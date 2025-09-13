namespace ForecastingGas.Dto.Requests;

public class BenchmarkParams
{
    public string AlgoType { get; set; } = "";
    public string ColumnName { get; set; } = "";
    public string LogTransform { get; set; } = "";
    public int SeasonLength { get; set; } = new();
}