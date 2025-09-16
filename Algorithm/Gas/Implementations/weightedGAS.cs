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
        ALgoOutput result = new();

        result.AlgoType = "GAS";

        var newHwesParams = new HwesParams();

        int data = hwesParams.ActualValues.Count;
        result.ActualValues = hwesParams.ActualValues;

        result.ColumnName = gasRequest.ColumnName;
        int seasonLength = Math.Max(2, hwesParams.ActualValues.Count / 10);

        var optimalParams = _search.GridSearchHWES(hwesParams.ActualValues, seasonLength);

        result.AlphaHwes = optimalParams.alpha;
        result.Beta = optimalParams.beta;
        result.Gamma = optimalParams.gamma;

        decimal alphaSes = _search.GenerateOptimalAlpha(hwesParams.ActualValues);

        List<decimal> windowedData = hwesParams.ActualValues
            .Take(data)
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

        List<decimal> copyData = windowedData.ToList();

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

        result.AlphaSes = forecastSes.alpha;

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

        decimal mseHwes = _error.CalculateMse(hwesError);
        decimal mseSes = _error.CalculateMse(sesError);

        var weights = _model.CalculateWeights(mseSes, mseHwes);

        var forecast = _model.GasWeightedForecast(forecastSes.forecast!, forecastHwes.ForecastValues, weights.weightSes, weights.weightHwes);

        result.ForecastValues.AddRange(forecast);
        result.SeasonalValues.AddRange(forecastHwes.SeasonalValues);
        result.TrendValues.AddRange(forecastHwes.TrendValues);
        result.LevelValues.AddRange(forecastHwes.LevelValues);

        if (gasRequest.AddPrediction.Trim().ToLower() == "yes")
        {
            hwesParams.ForecasHorizon = hwesParams.ForecasHorizon <= 0 ? 1
            : hwesParams.ForecasHorizon;

            var finalHwesParams = new HwesParams
            {
                SeasonLength = hwesParams.SeasonLength,
                ForecasHorizon = hwesParams.ForecasHorizon,
                LevelValues = result.LevelValues,
                TrendValues = result.TrendValues,
                SeasonalValues = result.SeasonalValues,
                PredictionValues = hwesParams.PredictionValues,
            };

            List<decimal> pred1 = _hwes.GenerateForecasts(finalHwesParams);
            result.PredictionValues.AddRange(pred1);

            for (int i = 0; i < hwesParams.ForecasHorizon; i++)
            {
                decimal prediction = new();
                if (i == 0)
                {
                    prediction = alphaSes * result.PredictionValues[i] + (1 - alphaSes) * result.ForecastValues.Last();
                    result.PredictionValues2.Add(prediction);
                }
                else
                    result.PredictionValues2.Add(alphaSes * result.PredictionValues[i] + (1 - alphaSes) * result.PredictionValues2[i - 1]);
            }
        }

        result.PreditionValuesAverage = result.PredictionValues
            .Zip(result.PredictionValues2, (a, b) => (a + b) / 2)
            .ToList();


        result.TimeComputed = _watch.StopWatch();

        // _log.LogInformation($"Total Count");
        // _log.LogInformation($"Actual Values: {hwesParams.ActualValues.Count}");
        // _log.LogInformation($"Forecast Values: {gasForecast.Count}");
        // _log.LogInformation($"Level: {levelValues.Count}");
        // _log.LogInformation($"Trend: {trendValues.Count}");
        // _log.LogInformation($"Seasonal: {seasonalValues.Count}");
        // _log.LogInformation($"Prediction 1: {GasPrediction.Count}");
        // _log.LogInformation($"Prediction 2: {GasPrediction2.Count}");
        // _log.LogInformation($"Prediction 3: {averaged.Count}");
        // _log.LogInformation($"Alpha Ses: {alphaSes}");
        // _log.LogInformation($"Alpha Hwes: {optimalParams.alpha}");
        // _log.LogInformation($"Beta: {optimalParams.beta}");
        // _log.LogInformation($"Gamma: {optimalParams.gamma}");

        return result;
    }
}