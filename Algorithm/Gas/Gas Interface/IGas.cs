using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm.Gas.Interface;

public interface IMtGas
{
    ALgoOutput ApplyMtGas(HwesParams hwesParams, SesParams sesParams, GasRequest gasRequest);
}

public interface IModel
{
    (decimal weightSes, decimal weightHwes) CalculateWeights(decimal mseSes, decimal mseHwes);
    List<decimal> GasWeightedForecast(List<decimal> fSes, List<decimal> fHwes, decimal weightSes, decimal weightHwes);
}