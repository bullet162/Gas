using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm.Gas.Interface;

public interface IMtGas
{
    ALgoOutput ApplyMtGas(HwesParams hwesParams, GasRequest gasRequest);

}

public interface IEnhanceGAS
{
    ALgoOutput ApplyAdaptiveGas
     (List<decimal> actualValues, int seasonalityLength, int forecastHorizon, int localWindow, string addPrediction);
}
public interface IModel
{
    (decimal weightSes, decimal weightHwes) CalculateWeights(decimal mseSes, decimal mseHwes);
    List<decimal> GasWeightedForecast(List<decimal> fSes, List<decimal> fHwes, decimal weightSes, decimal weightHwes);
}