using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm.Interfaces;

public interface ISes
{
    (List<decimal> trainedForecast, string model, int totalCount) SesForecast(SesParams ses);
}

public interface IHwes
{
    (List<decimal> trainedForecast, string model, int totalCount) TrainForecast(HwesParams hwesParams);
    List<decimal> GenerateForecasts(HwesParams hwesParams);
}
public interface IMa
{
    List<decimal> MaForecast(List<decimal> actualValues, int window);
}
