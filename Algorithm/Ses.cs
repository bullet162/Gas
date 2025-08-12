using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Algorithm.Interfaces;

namespace ForecastingGas.Algorithm.Ses;

public class ForecastSes : ISes
{
    public (List<decimal> trainedForecast, string model, int totalCount) SesForecast(SesParams ses)
    {
        const string Name = "SES";
        var alpha = ses.Alpha;
        var data = ses.ActualValues;
        var output = new ALgoOutput();

        for (int i = 0; i < data.Count; i++)
        {
            if (i == 0)
                output.ForecastValues.Add(data[0]);
            else
                output.ForecastValues.Add(alpha * data[i] + (1 - alpha) * output.ForecastValues[i - 1]);
        }

        int TotalCount = output.ForecastValues.Count;

        return new(output.ForecastValues, Name, TotalCount);
    }
}
