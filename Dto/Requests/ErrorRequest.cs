namespace ForecastingGas.Dto.Requests;

public class ErrorParams
{
    public int SeasonLength { get; set; } = new();
    public List<decimal> ActualValues { get; set; } = new();
    public List<decimal> ForecastValues { get; set; } = new();
}