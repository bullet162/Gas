using System.ComponentModel.DataAnnotations;
namespace ForecastingGas.Dto.Requests;

public class HwesParams
{
    public decimal Alpha { get; set; } = new();

    public decimal Beta { get; set; } = new();

    public decimal Gamma { get; set; } = new();
    public int ForecasHorizon { get; set; } = new();
    public int SeasonLength { get; set; } = new();

    public List<decimal> ActualValues { get; set; } = new();
    public List<decimal> ForecastValues { get; set; } = new();
    public List<decimal> SeasonalValues { get; set; } = new();
    public List<decimal> TrendValues { get; set; } = new();
    public List<decimal> LevelValues { get; set; } = new();
    public List<decimal> PredictionValues { get; set; } = new();

    public string AddPrediction { get; set; } = string.Empty;

}

public class InputHwesController
{
    public string ColumnName { get; set; } = string.Empty;
    // public int Id { get; set; } = new();

    [Range(1, 10, ErrorMessage = "Prediction value must range from 1 to 10 only!")]
    public int ForecasHorizon { get; set; } = new();

    public int SeasonLength { get; set; } = new();
    public string AddPrediction { get; set; } = string.Empty;

}
