using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;


public class SesParams
{
    public decimal Alpha { get; set; } = new();

    [MinLength(21, ErrorMessage = "Actual Values must have atleast 21 values!")]
    public List<decimal> ActualValues { get; set; } = new();
}
public class InputSesController
{
    [Required]
    public string ColumnName { get; set; } = string.Empty;

    [Range(0.1, 0.9, ErrorMessage = "Alpha Value: (0.1 - 0.9)!")]
    public decimal Alpha { get; set; } = new();
}