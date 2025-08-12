using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm.Gas.Implementations;

public class PGas
{
    private ISes _ses;
    private IHwes _hwes;
    public PGas(ISes ses, IHwes hwes)
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

    public ALgoOutput ApplyPGas(HwesParams hwesParams, SesParams sesParams, GasRequest gasRequest)
    {
        List<decimal> gasForecast = new();
        const string model = "GAS";
        var data = Math.Max(hwesParams.ActualValues.Count, sesParams.ActualValues.Count);

        for (int i = gasRequest.LocalWindow; i < data; i++)
        {
            Parallel.For(
            gasRequest.LocalWindow,
            data,
            i =>
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

                var forecastSes = CalculateSes(newSesParams);
                var forecastHwes = CalculateHwes(newHwesParams);

            }
        );



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