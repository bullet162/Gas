using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class FileUpload
{
    public IFormFile File { get; set; } = null!;
}
