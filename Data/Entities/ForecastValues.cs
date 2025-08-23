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
    public List<PredictionValues> GetPredictionValues { get; set; } = null!;

    [Column(TypeName = "nvarchar(max)")]
    public List<decimal> LevelValues { get; set; } = new();

    [Column(TypeName = "nvarchar(max)")]
    public List<decimal> TrendValues { get; set; } = new();

    [Column(TypeName = "nvarchar(max)")]
    public List<decimal> SeasonalValues { get; set; } = new();

    public int SeasonLength { get; set; } = new();
    public string TimeComputed { get; set; } = string.Empty;
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

public class PredictionValues
{
    [Key]
    public int Id { get; set; }

    public decimal PredictionValue { get; set; }
    public int ForecastDescriptionID { get; set; }
    public ForecastDescription GetForecastDescription { get; set; } = null!;
}
