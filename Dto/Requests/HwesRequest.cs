using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class HwesParams
{
    public decimal Alpha { get; set; } = new();
    public decimal Beta { get; set; } = new();
    public decimal Gamma { get; set; } = new();
    public int ForecasHorizon { get; set; } = new();
    public int SeasonLength { get; set; } = new();

    // [MinLength(64, ErrorMessage = "Actual Values must have atleast 64 values for Hwes to train!")]
    public List<decimal> ActualValues { get; set; } = new();
    public List<decimal> ForecastValues { get; set; } = new();

}

public class InputHwesController
{
    public int Id { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Alpha Value: (0.1 - 0.9)!")]
    public decimal Alpha { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Beta Value: (0.1 - 0.9)!")]
    public decimal Beta { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Gamma Value: (0.1 - 0.9)!")]
    public decimal Gamma { get; set; } = new();

    [Range(1, 10, ErrorMessage = "Prediction value must range from 1 to 10 only!")]
    public int ForecasHorizon { get; set; } = new();

    public int SeasonLength { get; set; } = new();
}
