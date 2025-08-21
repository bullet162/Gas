namespace ForecastingGas.Dto.Requests;

public class Neuron
{
    public List<double> Weights { get; set; } = null!;
    public double Bias { get; set; }
}

public class Layer
{
    public List<Neuron> Neurons { get; set; } = null!;
    public List<double> Biases => Neurons.Select(n => n.Bias).ToList();
}

public class NNInputs
{
    public List<decimal> Forecast { get; set; } = new();
    public List<decimal> ActualValues { get; set; } = new();
    public List<decimal> SeasonalValues { get; set; } = new();
    public List<decimal> TrendValues { get; set; } = new();
    public List<decimal> LevelValues { get; set; } = new();
    public List<decimal> Residuals { get; set; } = new();

}