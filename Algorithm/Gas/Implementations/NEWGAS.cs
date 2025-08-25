using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Algorithm.Gas;

public class EnhancedGas : IEnhanceGAS
{
    private ISearch _s;
    private IHwes _h;
    private IWatch _w;
    public EnhancedGas(ISearch s, IHwes h, IWatch w)
    {
        _s = s;
        _h = h;
        _w = w;
    }

    public ALgoOutput ApplyAdaptiveGas
    (List<decimal> actualValues, int seasonalityLength, int forecastHorizon, int localWindow, string addPrediction)
    {
        _w.StartWatch();
        var data = actualValues;
        var seasonLength = seasonalityLength;
        var h = forecastHorizon;
        var w = localWindow;
        decimal phi = new();

        List<decimal> level = new();
        List<decimal> trend = new();
        List<decimal> season = new();

        List<decimal> newLevel = new();
        List<decimal> newTrend = new();
        List<decimal> newSeason = new();
        List<decimal> forecast = new();
        List<decimal> prediction = new();

        var startIndex = data.Count % w;

        for (int i = startIndex; i < data.Count; i += w)
        {
            var windowedData = data.Skip(i).Take(w).ToList();

            var bestHwesParamaters = _s.GridSearchHWES(windowedData, seasonLength);

            var hwesParameters = new HwesParams
            {
                ActualValues = windowedData,
                SeasonLength = Math.Min(windowedData.Count / 4, seasonLength),
                Alpha = bestHwesParamaters.alpha,
                Beta = bestHwesParamaters.beta,
                Gamma = bestHwesParamaters.gamma,
                ForecasHorizon = h,
                ForecastValues = new List<decimal>(),
                SeasonalValues = new List<decimal>(),
                TrendValues = new List<decimal>(),
                LevelValues = new List<decimal>(),
                AddPrediction = "no",
                PredictionValues = new List<decimal>()
            };

            var fitHwesData = _h.TrainForecast(hwesParameters);
            var alpha = hwesParameters.Alpha;
            var beta = hwesParameters.Beta;
            var gamma = hwesParameters.Gamma;
            season = fitHwesData.SeasonalValues.ToList();
            trend = fitHwesData.TrendValues.ToList();
            level = fitHwesData.LevelValues.ToList();

            var tStrenght = TrendStrength(level, trend, hwesParameters.SeasonLength);
            var sStrengthAndNoise = SeasonalityStrength(windowedData, level, season, trend, hwesParameters.SeasonLength);
            var nStrength = NoiseLevel(sStrengthAndNoise.varNoise, windowedData, hwesParameters.SeasonLength);

            var currentPhi = (tStrenght + (1 - sStrengthAndNoise.sStrength) + (1 - nStrength)) / 3;
            phi = (decimal)Math.Max((double)0.01, (double)Math.Min((double)0.99, (double)currentPhi));

            if (i == startIndex)
            {
                var fV = fitHwesData.ForecastValues.Take(seasonLength + 1);
                forecast.AddRange(fV);
            }

            for (int j = seasonLength + 1; j < windowedData.Count; j++)
            {
                var seasonIndex = j % (seasonLength + 1);
                if (j == i + seasonLength + 1)
                {
                    var e = windowedData[j] - (level[j - 1] + phi * trend[j - 1] + season[j - 1]);
                    var l = level[j - 1] + phi * trend[j - 1] + alpha * e;
                    var t = beta * trend[j - 1] + phi * gamma * e;
                    var s = season[seasonIndex] + (1 - phi) * gamma * e;

                    newLevel.Add(l);
                    newTrend.Add(t);
                    newSeason.Add(s);
                }
                else
                {
                    var e = windowedData[j] - (level[j - 1] + phi * trend[j - 1] + season[j - 1]);
                    var l = level[j - 1] + phi * trend[j - 1] + alpha * e;
                    var t = beta * trend[j - 1] + phi * gamma * e;
                    var s = season[seasonIndex] + (1 - phi) * gamma * e;

                    newLevel.Add(l);
                    newTrend.Add(t);
                    newSeason.Add(s);

                    int newIndex = j - (seasonLength + 1);
                    var f = newLevel[newIndex] + phi * newTrend[newIndex] + newSeason[newIndex];

                    forecast.Add(f);
                }
            }
        }

        if (addPrediction.Trim().ToLower() == "yes")
        {
            for (int i = 1; i <= h; i++)
            {
                var previousSeasonIndex = (i + h) % seasonLength;
                var p = newLevel[^1] + phi * h * newTrend[^1] + newSeason[previousSeasonIndex];
                prediction.Add(p);
            }
        }

        var timeComputed = _w.StopWatch();
        return new ALgoOutput
        {
            ForecastValues = forecast,
            ActualValues = data,
            TotalCount = forecast.Count,
            AlgoType = "EnhancedGas",
            LevelValues = newLevel,
            TrendValues = newTrend,
            SeasonalValues = newSeason,
            SeasonLength = seasonLength,
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

        for (int i = s; i < n; i++)
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