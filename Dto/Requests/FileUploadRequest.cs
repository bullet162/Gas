using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ForecastingGas.Dto.Requests;

public class FileUpload
{
    public IFormFile File { get; set; } = null!;
}

public class ColumnNames
{
    public List<string> ColumnsNames { get; set; } = new();
}

public class SelectedColumnName
{
    public string SColumnName { get; set; } = string.Empty;
}