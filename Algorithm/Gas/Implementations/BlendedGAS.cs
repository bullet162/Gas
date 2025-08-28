using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Entities;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Error_Metrics.Interfaces;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Algorithm.Gas.Implementations;


public class BlendedGAS : IBlend
{
    private ISes _ses;
    private IHwes _hwes;
    private IError _error;
    private IModel _model;
    private ISearch _search;
    private IWatch _watch;
    public BlendedGAS(ISes ses, IHwes hwes, IError error, IModel model, ISearch search, IWatch watch)
    {
        _ses = ses;
        _hwes = hwes;
        _error = error;
        _model = model;
        _search = search;
        _watch = watch;
    }

    public List<decimal> CalculateSes(SesParams ses)
    {
        var alpha = _search.GenerateOptimalAlpha(ses.ActualValues);
        var results = _ses.SesForecast(alpha, ses.ActualValues, ses.ForecastHorizon);

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

    public ALgoOutput ApplyBGas(HwesParams hwesParams)
    {
        _watch.StartWatch();
        List<decimal> gasForecast = new();
        List<decimal> seasonalValues = new();
        List<decimal> trendValues = new();
        List<decimal> levelValues = new();
        List<decimal> prediction = new();
        decimal alpha = new();
        decimal beta = new();
        decimal gamma = new();
        const string model = "BlendedGAS";
        var newHwesParams = new HwesParams();
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

            var optimalParams = _search.GridSearchHWES(windowedData, hwesParams.SeasonLength);
            alpha = optimalParams.alpha;
            beta = optimalParams.beta;
            gamma = optimalParams.gamma;
            newHwesParams = new HwesParams
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

            var copyData = windowedData.ToList();
            var newSesParams = new SesParams
            {
                ActualValues = copyData,
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

        var tStrength = TrendStrength(levelValues, trendValues, hwesParams.SeasonLength);
        var sStrength = SeasonalityStrength(hwesParams.ActualValues, levelValues, seasonalValues, trendValues, hwesParams.SeasonLength);
        var noise = NoiseLevel(sStrength.varNoise, hwesParams.ActualValues, hwesParams.SeasonLength);
        var currentPhi = (tStrength + (1 - sStrength.sStrength) + (1 - noise)) / 3;
        var phi = (decimal)Math.Max((double)0.01, (double)Math.Min((double)0.99, (double)currentPhi));
        var n = Math.Min(trendValues.Count, seasonalValues.Count);
        List<decimal> newLevel = new();
        List<decimal> newTrend = new();
        List<decimal> newSeason = new();

        for (int i = hwesParams.SeasonLength + 1; i < n; i++)
        {
            var seasonIndex = i % hwesParams.SeasonLength + 1;
            var e = hwesParams.ActualValues[i] - (levelValues[i - 1] + phi * trendValues[i - 1] + seasonalValues[seasonIndex]);
            var l = levelValues[i - 1] + phi * trendValues[i - 1] + alpha * e;
            var t = beta * trendValues[i - 1] + phi * gamma * e;
            var s = seasonalValues[seasonIndex] + (1 - phi) * gamma * e;

            newLevel.Add(l);
            newTrend.Add(t);
            newSeason.Add(s);
        }
        for (int i = 1; i <= hwesParams.ForecasHorizon; i++)
        {
            var previousSeasonIndex = (i + hwesParams.ForecasHorizon) % hwesParams.SeasonLength + 1;
            var f = newLevel[^1] + phi * newTrend[^1] + newSeason[previousSeasonIndex];

            prediction.Add(f);
        }

        var timeComputed = _watch.StopWatch();
        return new ALgoOutput
        {
            ForecastValues = gasForecast,
            ActualValues = hwesParams.ActualValues,
            TotalCount = gasForecast.Count,
            AlgoType = model,
            LevelValues = newLevel,
            TrendValues = newTrend,
            SeasonalValues = newSeason,
            SeasonLength = hwesParams.SeasonLength,
            PredictionValues = prediction,
            TimeComputed = timeComputed
        };

    }


    private decimal NoiseLevel(decimal varRt, List<decimal> data, int seasonLength)
    {
        var n = data.Skip(seasonLength + 1).ToList();
        var dataMean = data.Average();
        var sd_Data = new List<decimal>();

        for (int i = 0; i < n.Count(); i++)
        {
            var sd = (decimal)Math.Pow((double)data[i] - (double)dataMean, 2);
            sd_Data.Add(sd);
        }

        var dataVariance = sd_Data.Average();

        return 1 - (varRt / dataVariance);
    }

    private (decimal sStrength, decimal varNoise) SeasonalityStrength
    (List<decimal> actualValues, List<decimal> level, List<decimal> seasonalValues, List<decimal> trend, int seasonLength)
    {
        var sn = seasonLength + 1;
        var data = actualValues.Skip(sn).ToList();

        var l = level;
        var t = trend;
        var s = seasonalValues;

        var dTrended = new List<decimal>();
        var remainder = new List<decimal>();

        for (int i = sn; i < data.Count; i++)
        {
            int sIndex = i % seasonLength;
            var dt = data[i] - (t[i] + l[i]);
            var rt = dt - s[sIndex];

            dTrended.Add(dt);
            remainder.Add(rt);
        }

        var dtMean = dTrended.Average();
        var rtMean = remainder.Average();
        var n = Math.Min(dTrended.Count, remainder.Count);

        var squaredDeviations_Dt = new List<decimal>();
        var squaredDeviations_Rt = new List<decimal>();

        for (int i = 0; i < n; i++)
        {
            var sd_Dt = (decimal)Math.Pow((double)dTrended[i] - (double)dtMean, 2);
            var sd_Rt = (decimal)Math.Pow((double)remainder[i] - (double)rtMean, 2);

            squaredDeviations_Dt.Add(sd_Dt);
            squaredDeviations_Rt.Add(sd_Rt);
        }

        var dtVariance = squaredDeviations_Dt.Average();
        var rtVariance = squaredDeviations_Rt.Average();

        return new(1 - (rtVariance / dtVariance), rtVariance);
    }

    private decimal TrendStrength(List<decimal> level, List<decimal> trend, int seasonLength)
    {
        var s = seasonLength + 1;
        var l = level;
        var t = trend;
        var ct = new List<decimal>();
        var lMean = l.Average();

        for (int i = s; i <= t.Count; i++)
        {
            var cSum = t.Take(i).Sum();
            ct.Add(cSum);
        }

        var ctMean = ct.Average();
        var n = Math.Min(l.Count, ct.Count);

        var numerator = new List<decimal>();
        var denominatorOne = new List<decimal>();
        var denominatorTwo = new List<decimal>();

        for (int i = 0; i < n; i++)
        {
            var nume = (l[i] - lMean) * (ct[i] - ctMean);
            var denoOne = Math.Pow((double)l[i] - (double)lMean, 2);
            var denoTwo = Math.Pow((double)ct[i] - (double)ctMean, 2);

            numerator.Add(nume);
            denominatorOne.Add((decimal)denoOne);
            denominatorTwo.Add((decimal)denoTwo);
        }

        var numeSum = numerator.Sum();
        var denoOneSum = denominatorOne.Sum();
        var denoTwoSum = denominatorTwo.Sum();

        var r = (double)numeSum / Math.Sqrt((double)denoOneSum * (double)denoTwoSum);

        return (decimal)Math.Pow(r, 2);
    }
}