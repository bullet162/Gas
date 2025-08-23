using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm.Interfaces;

public interface ISes
{
    ALgoOutput SesForecast(decimal alphA, List<decimal> datA, int forecastHorizon);
}

public interface ISearch
{
    decimal GenerateOptimalAlpha(List<decimal> actualValues);
    (decimal alpha, decimal beta, decimal gamma, decimal mse) GridSearchHWES(
    IHwes hwes,
    List<decimal> actualData,
    int seasonLength,
    int steps = 10);

}

public interface IHwes
{
    ALgoOutput TrainForecast(HwesParams hwesParams, string addPrediction);
    List<decimal> GenerateForecasts(HwesParams hwesParams);
}
public interface IMa
{
    List<decimal> MaForecast(List<decimal> actualValues, int window);
}

public interface ITrainTest
{
    (List<decimal> Train, List<decimal> Test) SplitDataTwo(List<decimal> ActualValues);
}