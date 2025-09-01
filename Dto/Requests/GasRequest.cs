using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class GasRequest
{
    public string ColumnName { get; set; } = string.Empty;
    public string AddPrediction { get; set; } = string.Empty;
}

public class GasRequestController
{
    public string ColumnName { get; set; } = string.Empty;

    public string LogTransform { get; set; } = "";

}