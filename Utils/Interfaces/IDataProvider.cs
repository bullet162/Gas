namespace ForecastingGas.Utils.Interfaces;

public interface IDataProvider
{
    List<decimal> RandomGenerator(int count);
}