using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class GasRequest
{
    public decimal AlphaSes { get; set; } = new();
    public string ColumnName { get; set; } = string.Empty;
    public string AddPrediction { get; set; } = string.Empty;
}

public class GasRequestController
{
    public string ColumnName { get; set; } = string.Empty;

}