namespace ForecastingGas.Dto.Requests;

public class GetForecast
{
    public string AlgoType { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime DateForecasted { get; set; }
}