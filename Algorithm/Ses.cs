using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Algorithm.Ses;

public class ForecastSes : ISes
{
    private IWatch _watch;
    public ForecastSes(IWatch watch)
    {
        _watch = watch;
    }

    public ALgoOutput SesForecast(decimal alpha, List<decimal> datA, int forecastHorizon)
    {
        _watch.StartWatch();

        if (datA == null || datA.Count == 0)
            throw new ArgumentNullException("Actual Values cannot be Null!");

        forecastHorizon = forecastHorizon == 0 ? 1 : forecastHorizon;

        const string Name = "SES";
        var data = datA;
        var prediction = new decimal();
        var output = new ALgoOutput();

        for (int i = 0; i < data.Count; i++)
        {
            if (i == 0)
                output.ForecastValues.Add(data[0]);
            else
                output.ForecastValues.Add(alpha * data[i] + (1 - alpha) * output.ForecastValues[i - 1]);
        }

        for (int i = 0; i < forecastHorizon; i++)
        {
            prediction = alpha * data[^1] + (1 - alpha) * output.ForecastValues[^1];
            output.PredictionValues.Add(prediction);
        }

        int TotalCount = output.ForecastValues.Count;
        string timeComputed = _watch.StopWatch();
        var result = new ALgoOutput
        {
            ForecastValues = output.ForecastValues,
            ActualValues = data,
            AlgoType = Name,
            TotalCount = TotalCount,
            PredictionValues = output.PredictionValues,
            TimeComputed = timeComputed,
            AlphaSes = alpha
        };

        return result;
    }
}
