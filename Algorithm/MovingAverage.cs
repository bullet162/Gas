namespace ForecastingGas.Algorithm.MA;

public class MovingAverage
{
    public List<decimal> MaForecast(List<decimal> actualValues, int window)
    {
        var result = new List<decimal>();

        for (int i = window; i < actualValues.Count; i++)
        {
            decimal sum = actualValues.Skip(i - window).Take(window).Sum();

            result.Add(sum / window);
        }

        return result;
    }
}