using ForecastingGas.Algorithm.Gas.Interface;

namespace ForecastingGas.Algorithm.Gas.Implementations.Utils;

public class WeightedForecast : IModel
{
    public (decimal weightSes, decimal weightHwes) CalculateWeights(decimal mseSes, decimal mseHwes)
    {
        var weightSes = 1 / mseSes / ((1 / mseSes) + (1 / mseHwes));
        var weightHwes = 1 / mseHwes / ((1 / mseHwes) + (1 / mseSes));

        return new(weightSes, weightHwes);
    }


    public List<decimal> GasWeightedForecast(List<decimal> fSes, List<decimal> fHwes, decimal weightSes, decimal weightHwes)
    {
        var forecast = new List<decimal>();


        for (int i = 0; i < fSes.Count; i++)
        {
            forecast.Add((weightSes * fSes[i]) + (weightHwes * fHwes[i]));
        }
        return forecast;
    }
}