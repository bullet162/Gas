namespace ForecastingGas.Dto.Requests;

public class ErrorParams
{
    public int SeasonLength { get; set; } = new();
    public List<decimal> ActualValues { get; set; } = new();
    public List<decimal> ForecastValues { get; set; } = new();
}

public class CalcErrors
{
    public string ColumnName { get; set; } = string.Empty;
    public string AlgoType { get; set; } = string.Empty;
}

public class ErrorEvaluate
{
    public List<decimal> ActualValues { get; set; } = new();
    public List<decimal> ForecastValues { get; set; } = new();
}