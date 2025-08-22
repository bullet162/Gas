using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Algorithm.Interfaces;

namespace ForecastingGas.Algorithm.Ses;

public class ForecastSes : ISes
{
    public ALgoOutput SesForecast(decimal alphA, List<decimal> datA)
    {
        const string Name = "SES";
        var alpha = alphA;
        var data = datA;
        var output = new ALgoOutput();

        for (int i = 0; i < data.Count; i++)
        {
            if (i == 0)
                output.ForecastValues.Add(data[0]);
            else
                output.ForecastValues.Add(alpha * data[i] + (1 - alpha) * output.ForecastValues[i - 1]);
        }

        int TotalCount = output.ForecastValues.Count;

        var result = new ALgoOutput
        {
            ForecastValues = output.ForecastValues,
            ActualValues = data,
            AlgoType = Name,
            TotalCount = TotalCount
        };

        return result;
    }
}
