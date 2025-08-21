namespace ForecastingGas.Algorithm;

public class TrainTest
{
    public (List<decimal> Train, List<decimal> Test) SplitDataTwo(List<decimal> ActualValues)
    {

        var trainSize = (int)(ActualValues.Count * 0.8);
        var train = ActualValues.Take(trainSize).ToList();
        var test = ActualValues.Skip(trainSize).ToList();

        return new(train, test);
    }

    public (List<decimal> Train, List<decimal> Validation, List<decimal> Test) SplitDataThree(List<decimal> ActualValues)
    {
        var trainSize = (int)(ActualValues.Count * 0.54);
        var validationSize = (int)(ActualValues.Count * 0.26);

        var train = ActualValues.Take(trainSize).ToList();
        var validation = ActualValues.Skip(trainSize).Take(validationSize).ToList();
        var test = ActualValues.Skip(trainSize + validationSize).ToList();

        return new(train, validation, test);
    }
}