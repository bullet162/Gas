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
        decimal alpha = _search.GenerateOptimalAlpha(ses.ActualValues);

        var results = _ses.SesForecast(alpha, ses.ActualValues, ses.ForecastHorizon);

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
            Alpha = new decimal()
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
                Gamma = optimalParams.gamma
            };

            List<decimal> pred1 = Prediction1(finalHwesParams);
            result.PredictionValues.AddRange(pred1);

            for (int i = 0; i < hwesParams.ForecasHorizon; i++)
            {
                decimal prediction = new();
                if (i == 0)
                {
                    prediction = result.AlphaSes * result.PredictionValues[i] + (1 - result.AlphaSes) * result.ForecastValues.Last();
                    result.PredictionValues2.Add(prediction);
                }
                else
                    result.PredictionValues2.Add(result.AlphaSes * result.PredictionValues[i] + (1 - result.AlphaSes) * result.PredictionValues2[i - 1]);
            }
        }

        result.PreditionValuesAverage = result.PredictionValues
            .Zip(result.PredictionValues2, (a, b) => (a + b) / 2)
            .ToList();


        result.SeasonLength = hwesParams.SeasonLength;
        result.TimeComputed = _watch.StopWatch();

        return result;
    }


    private List<decimal> Prediction1(HwesParams hwesParams)
    {
        List<decimal> _level = hwesParams.LevelValues;
        List<decimal> _trend = hwesParams.TrendValues;
        List<decimal> _seasonal = hwesParams.SeasonalValues;
        int _seasonLength = hwesParams.SeasonLength;

        var horizon = hwesParams.ForecasHorizon;
        var forecasts = hwesParams.PredictionValues;
        for (int i = 1; i <= horizon; i++)
        {
            if (_seasonal.Count < _seasonLength || _level.Count == 0 || _trend.Count == 0)
                throw new InvalidOperationException("Model must be trained before generating forecasts.");

            int seasonIndex = (_seasonal.Count - _seasonLength + (i % _seasonLength)) % _seasonLength;

            if (_seasonLength >= 1)
                forecasts.Add(_level[^1] + i * _trend[^1] + _seasonal[seasonIndex]);

            else
                forecasts.Add(_level[^1] + i * _trend[^1]);
        }

        return forecasts;
    }
}