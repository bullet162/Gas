using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForecastingGas.Data.Entities;

public class ForecastDescription
{
    [Key]
    public int Id { get; set; }

    public string AlgoType { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; } = DateTime.UtcNow;
    public int TotalCount { get; set; }
    public List<ForecastValues> GetForecastValues { get; set; } = null!;

}
public class ForecastValues
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal ForecastValue { get; set; }
    public int ForecastDescriptionID { get; set; }
    public ForecastDescription GetForecastDescription { get; set; } = null!;
}