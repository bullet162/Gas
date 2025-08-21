// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;
// using ForecastingGas.Dto.Requests;

// namespace ForecastingGas.Data.Entities;

// public class NeuroGASForecastDescription
// {
//     [Key]
//     public int Id { get; set; }
//     public string ColumnName { get; set; } = string.Empty;
//     public DateTime ForecastDate { get; set; } = DateTime.UtcNow;
//     public List<NeuroGASForecastValues> GetNeuroGASForecastValues { get; set; } = null!;
//     public int TotalForecastValuesCount { get; set; }

// }

// public class NeuroGASForecastValues
// {
//     [Key]
//     public int Id { get; set; }

//     [Column(TypeName = "decimal(18,9)")]
//     public decimal ForecastValue { get; set; }
//     public int NeuroGASForecastDescriptionID { get; set; }
//     public NeuroGASForecastDescription GetNeuroGASForecastDescription { get; set; } = null!;
// }

// public class NeuroGASWBDescription
// {
//     [Key]
//     public int Id { get; set; }
//     public List<Neurons> Neurons { get; set; } = null!;

// }

// public class Neurons
// {
//     [Key]
//     public int Id { get; set; }

//     public decimal Bias { get; set; }
//     public List<Weights> Weights { get; set; } = null!;
//     public int NeuroGASWBDescriptionID { get; set; }
//     public NeuroGASWBDescription GetNeuroGASWBDescription { get; set; } = null!;
// }

// public class Weights
// {
//     [Key]
//     public int Id { get; set; }

//     public double Weight { get; set; }
//     public int NeuronsId { get; set; }
//     public Neurons GetNeurons { get; set; } = null!;
// }