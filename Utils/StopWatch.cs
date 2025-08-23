using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Utils;

public class Watch : IWatch
{
    System.Diagnostics.Stopwatch stopWatch = new();

    public void StartWatch()
    {
        stopWatch.Start();
    }

    public string StopWatch()
    {
        stopWatch.Stop();
        TimeSpan elapsed = stopWatch.Elapsed;

        return $"{stopWatch.Elapsed.TotalSeconds}s";
    }
}