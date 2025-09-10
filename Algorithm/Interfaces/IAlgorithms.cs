using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm.Interfaces;

public interface ISes
{
    ALgoOutput SesForecast(decimal alpha, List<decimal> datA, int forecastHorizon);
}

public interface ISearch
{
    decimal GenerateOptimalAlpha(List<decimal> actualValues);
    (decimal alpha, decimal beta, decimal gamma, decimal mse) GridSearchHWES(
    List<decimal> actualData,
    int seasonLength,
    int steps = 10);

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

public interface ITrainTest
{
    (List<decimal> Train, List<decimal> Test) SplitDataTwo(List<decimal> ActualValues);
    List<decimal> Cut(List<decimal> ActualValues);
}

public interface IProcessing
{
    List<decimal> LogTransformation(List<decimal> ActualValues);
    List<decimal> BackLogTransform(List<decimal> LogValues);
}

public interface ISortPredictions
{
    PredictionsResult SortForecast(PredictionsResult predictions);
}