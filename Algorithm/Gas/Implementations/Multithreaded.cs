using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Entities;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Error_Metrics.Interfaces;

namespace ForecastingGas.Algorithm.Gas.Implementations;

/*
SES: Starts forecasting right at index 0
HWES: Starts forecasting right at index seasonLength + 2
*/

public class MTGas : IMtGas
{
    private ISes _ses;
    private IHwes _hwes;
    private IError _error;
    private IModel _model;
    private ISearch _search;
    public MTGas(ISes ses, IHwes hwes, IError error, IModel model, ISearch search)
    {
        _ses = ses;
        _hwes = hwes;
        _error = error;
        _model = model;
        _search = search;
    }

    public List<decimal> CalculateSes(SesParams ses)
    {
        var alpha = _search.GenerateOptimalAlpha(ses.ActualValues);
        var results = _ses.SesForecast(alpha, ses.ActualValues);

        return results.ForecastValues;
    }

    public ALgoOutput CalculateHwes(HwesParams hwesParams)
    {

        var results = _hwes.TrainForecast(hwesParams);

        return new ALgoOutput
        {
            ForecastValues = results.ForecastValues,
            SeasonalValues = results.SeasonalValues,
            LevelValues = results.LevelValues,
            TrendValues = results.TrendValues
        };
    }

    public ALgoOutput ApplyMtGas(HwesParams hwesParams, GasRequest gasRequest)
    {
        List<decimal> gasForecast = new();
        List<decimal> seasonalValues = new();
        List<decimal> trendValues = new();
        List<decimal> levelValues = new();
        const string model = "GAS";

        int windowSize = hwesParams.ActualValues.Count / 2;
        hwesParams.SeasonLength = Math.Max(2, hwesParams.ActualValues.Count / 10);

        if (windowSize < hwesParams.SeasonLength * 2)
            throw new ArgumentException(
                $"Window size ({windowSize}) too small for seasonLength {hwesParams.SeasonLength}. " +
                "Need at least 2 × seasonLength points.");

        for (int end = windowSize; end <= hwesParams.ActualValues.Count; end++)
        {
            var windowedData = hwesParams.ActualValues
                .Skip(end - windowSize)
                .Take(windowSize)
                .ToList();

            var optimalParams = _search.GridSearchHWES(_hwes, windowedData, hwesParams.SeasonLength, steps: 10);

            var newHwesParams = new HwesParams
            {
                ActualValues = windowedData,
                Alpha = optimalParams.alpha,
                Beta = optimalParams.beta,
                Gamma = optimalParams.gamma,
                SeasonLength = Math.Max(2, Math.Min(hwesParams.SeasonLength, windowedData.Count / 2)),
                ForecasHorizon = hwesParams.ForecasHorizon,
                ForecastValues = new List<decimal>(),
                SeasonalValues = new List<decimal>(),
                LevelValues = new List<decimal>(),
                TrendValues = new List<decimal>()
            };

            var newSesParams = new SesParams
            {
                ActualValues = windowedData,
                Alpha = new decimal()
            };

            var forecastSes = new List<decimal>();
            var forecastHwes = new ALgoOutput();

            Parallel.Invoke(
                () => forecastSes = CalculateSes(newSesParams),
                () => forecastHwes = CalculateHwes(newHwesParams)
            );

            var sesError = new ErrorParams
            {
                ActualValues = windowedData,
                ForecastValues = forecastSes,
                SeasonLength = hwesParams.SeasonLength
            };

            var hwesError = new ErrorParams
            {
                ActualValues = windowedData,
                ForecastValues = forecastHwes.ForecastValues,
                SeasonLength = hwesParams.SeasonLength
            };

            var mseHwes = _error.CalculateMse(hwesError);
            var mseSes = _error.CalculateMse(sesError);

            var weights = _model.CalculateWeights(mseSes, mseHwes);

            var forecast = _model.GasWeightedForecast(forecastSes, forecastHwes.ForecastValues, weights.weightSes, weights.weightHwes);

            if (end == windowSize)
            {
                gasForecast.AddRange(forecast);
                seasonalValues.AddRange(forecastHwes.SeasonalValues);
                trendValues.AddRange(forecastHwes.TrendValues);
                levelValues.AddRange(forecastHwes.LevelValues);
            }
            else if (forecast.Any())
            {
                gasForecast.Add(forecast.Last());
                if (forecastHwes.SeasonalValues.Any())
                    seasonalValues.Add(forecastHwes.SeasonalValues.Last());
                if (forecastHwes.TrendValues.Any())
                    trendValues.Add(forecastHwes.TrendValues.Last());
                if (forecastHwes.LevelValues.Any())
                    levelValues.Add(forecastHwes.LevelValues.Last());
            }

        }

        return new ALgoOutput
        {
            ForecastValues = gasForecast,
            ActualValues = hwesParams.ActualValues,
            ColumnName = gasRequest.ColumnName,
            TotalCount = gasForecast.Count,
            AlgoType = model,
            LevelValues = levelValues,
            TrendValues = trendValues,
            SeasonalValues = seasonalValues,
            SeasonLength = hwesParams.SeasonLength
        };

    }

}