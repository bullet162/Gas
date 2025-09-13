using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Entities;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Error_Metrics.Interfaces;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Algorithm.Gas.Implementations;


public class MTGas : IMtGas
{
    private ISes _ses;
    private IHwes _hwes;
    private IError _error;
    private IModel _model;
    private ISearch _search;
    private IWatch _watch;
    private readonly ILogger<MTGas> _log;
    public MTGas(ISes ses, IHwes hwes, IError error, IModel model, ISearch search, IWatch watch, ILogger<MTGas> log)
    {
        _ses = ses;
        _hwes = hwes;
        _error = error;
        _model = model;
        _search = search;
        _watch = watch;
        _log = log;
    }

    public (List<decimal> forecast, decimal alpha) CalculateSes(SesParams ses)
    {
        var results = _ses.SesForecast(0.1m, ses.ActualValues, ses.ForecastHorizon);

        return new(results.ForecastValues, results.AlphaSes);
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
        _watch.StartWatch();
        List<decimal> gasForecast = new();
        List<decimal> seasonalValues = new();
        List<decimal> trendValues = new();
        List<decimal> levelValues = new();
        List<decimal> GasPrediction = new();
        List<decimal> GasPrediction2 = new();
        (decimal alpha, decimal beta, decimal gamma, decimal mse) optimalParams = new();
        decimal alphaSes = new(); ;
        const string model = "GAS";
        var newHwesParams = new HwesParams();

        int windowSize = hwesParams.ActualValues.Count;
        int seasonLength = Math.Max(2, hwesParams.ActualValues.Count / 10);

        optimalParams = _search.GridSearchHWES(hwesParams.ActualValues, seasonLength);
        alphaSes = _search.GenerateOptimalAlpha(hwesParams.ActualValues);

        for (int end = windowSize; end <= hwesParams.ActualValues.Count; end++)
        {
            var windowedData = hwesParams.ActualValues
                .Skip(end - windowSize)
                .Take(windowSize)
                .ToList();

            newHwesParams = new HwesParams
            {
                ActualValues = windowedData,
                Alpha = optimalParams.alpha,
                Beta = optimalParams.beta,
                Gamma = optimalParams.gamma,
                SeasonLength = hwesParams.SeasonLength,
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
                Alpha = alphaSes
            };

            (List<decimal> forecast, decimal alpha) forecastSes = default;
            var forecastHwes = new ALgoOutput();

            Parallel.Invoke(
                () => forecastSes = CalculateSes(newSesParams),
                () => forecastHwes = CalculateHwes(newHwesParams)
            );

            alphaSes = forecastSes.alpha;

            var sesError = new ErrorParams
            {
                ActualValues = windowedData,
                ForecastValues = forecastSes.forecast!,
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

            var forecast = _model.GasWeightedForecast(forecastSes.forecast!, forecastHwes.ForecastValues, weights.weightSes, weights.weightHwes);

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

        if (gasRequest.AddPrediction.Trim().ToLower() == "yes")
        {
            hwesParams.ForecasHorizon = hwesParams.ForecasHorizon <= 0 ? 1
            : hwesParams.ForecasHorizon;

            var finalHwesParams = new HwesParams
            {
                SeasonLength = hwesParams.SeasonLength,
                ForecasHorizon = hwesParams.ForecasHorizon,
                LevelValues = levelValues,
                TrendValues = trendValues,
                SeasonalValues = seasonalValues,
                PredictionValues = hwesParams.PredictionValues,
            };

            List<decimal> pred1 = _hwes.GenerateForecasts(finalHwesParams);
            GasPrediction.AddRange(pred1);

            for (int i = 0; i < hwesParams.ForecasHorizon; i++)
            {
                decimal prediction = new();
                if (i == 0)
                {
                    prediction = alphaSes * GasPrediction[i] + (1 - alphaSes) * gasForecast.Last();
                    GasPrediction2.Add(prediction);
                }
                else
                    GasPrediction2.Add(alphaSes * GasPrediction[i] + (1 - alphaSes) * GasPrediction2[i - 1]);
            }
        }

        var averaged = GasPrediction
            .Zip(GasPrediction2, (a, b) => (a + b) / 2)
            .ToList();


        var timeComputed = _watch.StopWatch();

        _log.LogInformation($"Total Count");
        _log.LogInformation($"Actual Values: {hwesParams.ActualValues.Count}");
        _log.LogInformation($"Forecast Values: {gasForecast.Count}");
        _log.LogInformation($"Level: {levelValues.Count}");
        _log.LogInformation($"Trend: {trendValues.Count}");
        _log.LogInformation($"Seasonal: {seasonalValues.Count}");
        _log.LogInformation($"Prediction 1: {GasPrediction.Count}");
        _log.LogInformation($"Prediction 2: {GasPrediction2.Count}");
        _log.LogInformation($"Prediction 3: {averaged.Count}");
        _log.LogInformation($"Alpha Ses: {alphaSes}");
        _log.LogInformation($"Alpha Hwes: {optimalParams.alpha}");
        _log.LogInformation($"Beta: {optimalParams.beta}");
        _log.LogInformation($"Gamma: {optimalParams.gamma}");

        return new ALgoOutput
        {
            ForecastValues = gasForecast,
            ActualValues = hwesParams.ActualValues,
            ColumnName = gasRequest.ColumnName,
            TotalCount = GasPrediction.Count + GasPrediction2.Count + averaged.Count,
            AlgoType = model,
            LevelValues = levelValues,
            TrendValues = trendValues,
            SeasonalValues = seasonalValues,
            SeasonLength = hwesParams.SeasonLength,
            PredictionValues = GasPrediction,
            TimeComputed = timeComputed,
            PredictionValues2 = GasPrediction2,
            AlphaSes = alphaSes,
            AlphaHwes = optimalParams.alpha,
            Beta = optimalParams.beta,
            Gamma = optimalParams.gamma,
            PreditionValuesAverage = averaged
        };
    }
}