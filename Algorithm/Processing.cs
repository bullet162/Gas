using ForecastingGas.Algorithm.Interfaces;

namespace ForecastingGas.Algorithm;

public class Processing : IProcessing
{
    public List<decimal> LogTransformation(List<decimal> ActualValues)
    {
        if (ActualValues == null || ActualValues.Count == 0)
            throw new ArgumentNullException("Data cannot be null!");

        return ActualValues
            .AsParallel()
            .AsOrdered()
            .Select(d => (decimal)Math.Log10((double)d))
            .ToList();
    }

    public List<decimal> BackLogTransform(List<decimal> LogValues)
    {
        if (LogValues == null || LogValues.Count == 0)
            throw new ArgumentNullException("Data cannot be null!");

        return LogValues
            .AsParallel()
            .AsOrdered()
            .Select(x => (decimal)Math.Pow(10, (double)x))
            .ToList();
    }
}