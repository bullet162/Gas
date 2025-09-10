using System.Collections.Concurrent;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Algorithm;

public class GridSearch : ISearch
{
    private readonly ISes _ses;
    private readonly IHwes _hwes;

    public GridSearch(ISes ses, IHwes hwes)
    {
        _ses = ses;
        _hwes = hwes;
    }

    public decimal GenerateOptimalAlpha(List<decimal> actualValues)
    {
        var alphas = Enumerable.Range(1, 900).Select(i => 0.001m * i);

        var results = alphas
            .AsParallel()
            .AsOrdered()
            .Select(alpha =>
            {
                var forecast = _ses.SesForecast(alpha, actualValues, 0);

                var error = ComputeRmse(actualValues, forecast.ForecastValues);

                return new { Alpha = alpha, Error = error };
            })
            .ToList();

        var best = results.OrderBy(r => r.Error).First();

        return best.Alpha;
    }

    private decimal ComputeRmse(List<decimal> actual, List<decimal> forecast)
    {
        var n = actual.Count;
        var mse = actual.Zip(forecast, (a, f) => (a - f) * (a - f)).Average();
        return (decimal)Math.Sqrt((double)mse);
    }

    private decimal ComputeMSE(List<decimal> actual, List<decimal> forecast, int seasonLength)
    {
        int startIndex = seasonLength + 2;
        decimal mse = 0;
        int n = Math.Min(forecast.Count, actual.Count - startIndex);

        for (int i = 0; i < n; i++)
        {
            mse += (actual[i + startIndex] - forecast[i]) * (actual[i + startIndex] - forecast[i]);
        }

        return mse / n;
    }


    public (decimal alpha, decimal beta, decimal gamma, decimal mse) GridSearchHWES(
    List<decimal> actualData,
    int seasonLength,
    int steps = 10)
    {
        decimal stepSize = 1.0M / steps;
        var candidates = from i in Enumerable.Range(0, steps + 1)
                         from j in Enumerable.Range(0, steps + 1)
                         from k in Enumerable.Range(0, steps + 1)
                         select new { Alpha = i * stepSize, Beta = j * stepSize, Gamma = k * stepSize };

        var bestResult = new ConcurrentBag<(decimal alpha, decimal beta, decimal gamma, decimal mse)>();

        candidates
            .AsParallel()
            .ForAll(c =>
            {
                var hwesParams = new HwesParams
                {
                    ActualValues = actualData,
                    LevelValues = new List<decimal>(),
                    TrendValues = new List<decimal>(),
                    SeasonalValues = new List<decimal>(),
                    Alpha = c.Alpha,
                    Beta = c.Beta,
                    Gamma = c.Gamma,
                    SeasonLength = seasonLength,
                    ForecasHorizon = 0,
                    ForecastValues = new List<decimal>(),
                    AddPrediction = "no"
                };

                var forecast = _hwes.TrainForecast(hwesParams).ForecastValues;
                decimal mse = ComputeMSE(actualData, forecast, seasonLength);

                bestResult.Add((c.Alpha, c.Beta, c.Gamma, mse));
            });

        return bestResult.OrderBy(r => r.mse).First();
    }

}
