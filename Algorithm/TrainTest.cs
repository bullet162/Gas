using ForecastingGas.Algorithm.Interfaces;

namespace ForecastingGas.Algorithm;

public class TrainTest : ITrainTest
{
    public (List<decimal> Train, List<decimal> Test) SplitDataTwo(List<decimal> ActualValues)
    {
        var trainSize = (int)(ActualValues.Count * 0.75);
        var train = ActualValues.Take(trainSize).ToList();
        var test = ActualValues.Skip(trainSize).ToList();

        return new(train, test);
    }

    public List<decimal> Cut(List<decimal> ActualValues)
    {
        var trainSize = (int)(ActualValues.Count * 0.25);
        var data = ActualValues.Skip(trainSize).ToList();

        return data;
    }
}