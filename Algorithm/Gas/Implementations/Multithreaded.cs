using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;
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
    public MTGas(ISes ses, IHwes hwes)
    {
        _ses = ses;
        _hwes = hwes;
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

    public (List<decimal> forecastvalues, string model, int TotalCount) ApplyMtGas(HwesParams hwesParams, SesParams sesParams, GasRequest gasRequest)
    {
        var data = Math.Max(hwesParams.ActualValues.Count, sesParams.ActualValues.Count);

        for (int i = gasRequest.LocalWindow; i < data; i++)
        {
            var windowedData = hwesParams.ActualValues.Take(gasRequest.LocalWindow + i).ToList();

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

        }

        return new([], "", 2);
    }

}