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
    public MTGas(ISes ses, IHwes hwes, IError error, IModel model)
    {
        _ses = ses;
        _hwes = hwes;
        _error = error;
        _model = model;
    }

    public List<decimal> CalculateSes(SesParams ses)
    {
        var results = _ses.SesForecast(ses);

        return results.trainedForecast;
    }

    public List<decimal> CalculateHwes(HwesParams hwesParams)
    {
        var results = _hwes.TrainForecast(hwesParams);

        return results.trainedForecast;
    }

    public ALgoOutput ApplyMtGas(HwesParams hwesParams, SesParams sesParams, GasRequest gasRequest)
    {
        List<decimal> gasForecast = new();
        const string model = "GAS";

        var data = Math.Max(hwesParams.ActualValues.Count, sesParams.ActualValues.Count);

        for (int i = gasRequest.LocalWindow; i < data; i++)
        {
            var windowedData = hwesParams.ActualValues.Take(i).ToList();

            var newHwesParams = new HwesParams
            {
                ActualValues = windowedData,
                Alpha = hwesParams.Alpha,
                Beta = hwesParams.Beta,
                Gamma = hwesParams.Gamma,
                SeasonLength = hwesParams.SeasonLength,
                ForecasHorizon = hwesParams.ForecasHorizon,
                ForecastValues = hwesParams.ForecastValues
            };

            var newSesParams = new SesParams
            {
                ActualValues = windowedData,
                Alpha = sesParams.Alpha
            };

            var forecastSes = new List<decimal>();
            var forecastHwes = new List<decimal>();

            Thread ses = new Thread
            (
                () => forecastSes = CalculateSes(newSesParams)
            );

            Thread hwes = new Thread
            (
                () => forecastHwes = CalculateHwes(newHwesParams)
            );

            ses.Start();
            hwes.Start();

            ses.Join();
            hwes.Join();

            var sesError = new ErrorParams
            {
                ActualValues = windowedData,
                ForecastValues = forecastSes,
                SeasonLength = hwesParams.SeasonLength
            };

            var hwesError = new ErrorParams
            {
                ActualValues = windowedData,
                ForecastValues = forecastHwes,
                SeasonLength = hwesParams.SeasonLength
            };

            var mseHwes = _error.CalculateMse(hwesError);
            var mseSes = _error.CalculateMse(sesError);

            var weights = _model.CalculateWeights(mseSes, mseHwes);

            var forecast = _model.GasWeightedForecast(forecastSes, forecastHwes, weights.weightSes, weights.weightHwes);

            gasForecast.AddRange(forecast);
        }

        return new ALgoOutput
        {
            ForecastValues = gasForecast,
            ActualValues = hwesParams.ActualValues,
            ColumnName = gasRequest.ColumnName,
            TotalCount = gasForecast.Count,
            AlgoType = model
        };
    }

}