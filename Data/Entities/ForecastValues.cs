using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForecastingGas.Data.Entities;

public class ForecastDescription
{
    [Key]
    public int Id { get; set; }
    public bool isLogTransformed { get; set; }
    public string AlgoType { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; } = DateTime.UtcNow;
    public int TotalCount { get; set; }
    public double AlphaSes { get; set; }
    public double AlphaHwes { get; set; }
    public double Beta { get; set; }
    public double Gamma { get; set; }
    public List<ForecastValues> GetForecastValues { get; set; } = null!;
    public List<PredictionValues> GetPredictionValues { get; set; } = null!;
    public List<PredictionValues2> GetPredictionValues2 { get; set; } = null!;
    public List<PredictionValues3> GetPredictionValues3 { get; set; } = null!;
    public List<ErrorValues> GetErrorValues { get; set; } = null!;

    public int SeasonLength { get; set; } = new();
    public string TimeComputed { get; set; } = string.Empty;
}
public class ForecastValues
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal ForecastValue { get; set; }

    [ForeignKey("GetForecastDescription5")]
    public int ForecastDescriptionID { get; set; }
    public ForecastDescription GetForecastDescription { get; set; } = null!;
}

public class PredictionValues
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal PredictionValue { get; set; }

    [ForeignKey("ForecastDescription")]
    public int ForecastDescriptionID2 { get; set; }
    public ForecastDescription GetForecastDescription2 { get; set; } = null!;
}

public class PredictionValues2
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal PredictionValue2 { get; set; }

    [ForeignKey("ForecastDescription")]
    public int ForecastDescriptionID3 { get; set; }
    public ForecastDescription GetForecastDescription3 { get; set; } = null!;
}

public class PredictionValues3
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal PredictionValue3 { get; set; }

    [ForeignKey("ForecastDescription")]
    public int ForecastDescriptionID4 { get; set; }
    public ForecastDescription GetForecastDescription4 { get; set; } = null!;
}
