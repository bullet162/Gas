using ForecastingGas.Algorithm.Interfaces;

namespace ForecastingGas.Algorithm.Gas.Implementations;

public class MISDGAS
{
    public (int one, int two) ApplyGas()
    {
        int numOne = new();
        int numTwo = new();

        Parallel.Invoke(
            () => numOne = 1 + 1,
            () => numTwo = 2 * 2
        );

        return new(numOne, numTwo);
    }
}