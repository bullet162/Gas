using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Algorithm.Gas.Interface;

public interface IMtGas
{
    (List<decimal> forecastvalues, string model, int TotalCount) ApplyMtGas(HwesParams hwesParams, SesParams sesParams, GasRequest gasRequest);
}