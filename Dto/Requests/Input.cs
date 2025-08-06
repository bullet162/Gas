using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class SesParams
{
    public decimal Alpha { get; set; } = new();

    [MinLength(21, ErrorMessage = "Actual Values must have atleast 21 values!")]
    public List<decimal> ActualValues { get; set; } = new();
}
public class InputSesParams
{
    [Range(0.1, 0.9, ErrorMessage = "Alpha Value: (0.1 - 0.9)!")]
    public decimal Alpha { get; set; } = new();

    [Required]
    public int Id { get; set; } = new();
}

public class FileUpload
{
    public IFormFile File { get; set; } = null!;
}

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
    public List<decimal> Level { get; set; } = new();
    public List<decimal> Trend { get; set; } = new();
    public List<decimal> Seasonal { get; set; } = new();


}

public class InputHwesParams
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

    [MinLength(24, ErrorMessage = "Season Length must have atleast 24 values to initialize!!")]
    public int SeasonLength { get; set; } = new();
}

public class ErrorParams
{
    public int SeasonLength { get; set; } = new();
    public List<decimal> ActualValues { get; set; } = new();
    public List<decimal> ForecastValues { get; set; } = new();
}

public class GasParams
{
    [Range(0.1, 0.9, ErrorMessage = "Alpha Value: (0.1 - 0.9)!")]
    public decimal AlphaSes { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Alpha Value: (0.1 - 0.9)!")]
    public decimal AlphaHwes { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Beta Value: (0.1 - 0.9)!")]
    public decimal Beta { get; set; } = new();

    [Range(0.1, 0.9, ErrorMessage = "Gamma Value: (0.1 - 0.9)!")]
    public decimal Gamma { get; set; } = new();
    public int ForecasHorizon { get; set; } = new();

    [MinLength(24, ErrorMessage = "Season Length must have atleast 24 values!")]
    public int SeasonLength { get; set; } = new();

    [MinLength(260, ErrorMessage = "GAS Algorithm needs atleast 260 values!")]
    public List<decimal> ActualValues { get; set; } = new();

    public List<decimal> ForecastValues { get; set; } = new();

    [MinLength(54, ErrorMessage = "Window Size Should have atleast 54 values!")]
    public int WindowSize { get; set; } = new();
}