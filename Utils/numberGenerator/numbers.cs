using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Utils.numberGenerator;

public class Numbers : IDataProvider
{
    public List<decimal> RandomGenerator(int count)
    {
        List<decimal> syntheticDataset = new();
        Random random = new();

        for (int i = 0; i < count; i++)
        {
            var data = random.Next(100, 1000);
            syntheticDataset.Add(data);
        }

        return syntheticDataset;
    }



}