using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class GasRequest
{
    public string ColumnName { get; set; } = string.Empty;
    public string AddPrediction { get; set; } = string.Empty;
}

public class GasRequestController
{

    [Range(1, 10, ErrorMessage = "Prediction value must range from 1 to 10 only!")]
    public int ForecasHorizon { get; set; } = new();

    public string ColumnName { get; set; } = string.Empty;

    public string AddPrediction { get; set; } = string.Empty;

}