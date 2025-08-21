using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm.Interfaces;

public interface ISes
{
    ALgoOutput SesForecast(SesParams ses);
}

public interface IHwes
{
    ALgoOutput TrainForecast(HwesParams hwesParams);
    List<decimal> GenerateForecasts(HwesParams hwesParams);
}
public interface IMa
{
    List<decimal> MaForecast(List<decimal> actualValues, int window);
}
