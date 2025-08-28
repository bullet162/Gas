using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForecastingGas.Data.Entities;

public class ErrorValues
{
    [Key]
    public int Id { get; set; }

    public string ColumnName { get; set; } = string.Empty;
    public string AlgoType { get; set; } = string.Empty;
    public DateTime DateEvaluated { get; set; } = DateTime.Today;

    public double RMSE { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAE { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MAPE { get; set; }

    [Column(TypeName = "decimal(18,9)")]
    public decimal MSE { get; set; }

}