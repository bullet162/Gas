namespace ForecastingGas.Dto.Responses;

public class ALgoOutput
{
    public int Id { get; set; } = new();
    public List<decimal> ForecastValues { get; set; } = new();
    public List<decimal> ActualValues { get; set; } = new();
    public string ColumnName { get; set; } = string.Empty;
    public int TotalCount { get; set; } = new();
    public string AlgoType { get; set; } = string.Empty;
    public List<decimal> LevelValues { get; set; } = new();
    public List<decimal> TrendValues { get; set; } = new();
    public List<decimal> SeasonalValues { get; set; } = new();
    public int SeasonLength { get; set; } = new();
    public List<decimal> PredictionValues = new();
}

public class RawDataOutput
{
    public int Id { get; set; }
    public List<decimal> ActualValues { get; set; } = new();
    public int TotalCount { get; set; } = new();
    public string ColumnName { get; set; } = string.Empty;
    public DateTime DateOfEntry { get; set; }
}

public class ErrorOutput
{
    public string ColumnName { get; set; } = string.Empty;
    public string AlgoType { get; set; } = string.Empty;
    public double RMSE { get; set; } = new();
    public decimal MAE { get; set; } = new();
    public decimal MAPE { get; set; } = new();
    public decimal MSE { get; set; } = new();
}