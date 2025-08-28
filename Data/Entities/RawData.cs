using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForecastingGas.Data.Entities;


public class DataDescription
{
    [Key]
    public int Id { get; set; }

    public string ColumnName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public DateTime DateUploaded { get; set; } = DateTime.Today;
    public List<ActualValues> ActualValues { get; set; } = null!;
}

public class ActualValues
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal ActualValue { get; set; }
    public int DataDescriptionID { get; set; }
    public DataDescription GetDataDescription { get; set; } = null!;
}

