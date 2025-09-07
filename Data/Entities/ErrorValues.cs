using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data.Entities;

public class ErrorValues
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("ForecastDescription")]
    public int ForecastDescriptionIdError { get; set; }
    public ForecastDescription GetForecastDescription5 { get; set; } = null!;
    public string ColumnName { get; set; } = string.Empty;
    public string AlgoType { get; set; } = string.Empty;
    public bool isLogTransformed { get; set; }
    public DateTime DateEvaluated { get; set; } = DateTime.UtcNow;

    public double RMSE { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAE { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAPE { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MSE { get; set; }

    public double RMSE2 { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAE2 { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAPE2 { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MSE2 { get; set; }

    public double RMSE3 { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAE3 { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAPE3 { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MSE3 { get; set; }

}