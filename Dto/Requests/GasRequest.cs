using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class GasRequest
{
    [Range(1, 10, ErrorMessage = "Forecast horizon range: (1 - 10)!")]
    public int ForecastHorizon { get; set; } = new();
    public int LocalWindow { get; set; } = new();
    public string ColumnName { get; set; } = string.Empty;
}

public class GasRequestController
{
    public int Id { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Alpha Value: (0.1 - 0.9)!")]
    public decimal AlphaSes { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Alpha Value: (0.1 - 0.9)!")]
    public decimal AlphaHwes { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Beta Value: (0.1 - 0.9)!")]
    public decimal Beta { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Gamma Value: (0.1 - 0.9)!")]
    public decimal Gamma { get; set; } = new();

    [Range(1, 10, ErrorMessage = "Prediction value must range from 1 to 10 only!")]
    public int ForecasHorizon { get; set; } = new();

    public int SeasonLength { get; set; } = new();

}