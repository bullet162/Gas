namespace ForecastingGas.Dto.Responses;

public class ALgoOutput
{
    public List<decimal> ForecastValues { get; set; } = new();
    public List<decimal> ActualValues { get; set; } = new();
    public string ColumnName { get; set; } = string.Empty;
    public int TotalCount { get; set; } = new();
    public string AlgoType { get; set; } = string.Empty;
}

public class RawDataOutput
{
    public List<decimal> ActualValues { get; set; } = new();
    public int TotalCount { get; set; } = new();
    public string ColumnName { get; set; } = string.Empty;
}

public class ErrorOutput
{
    public double RMSE { get; set; } = new();
    public decimal MAE { get; set; } = new();
    public decimal MAPE { get; set; } = new();
    public decimal MSE { get; set; } = new();
}