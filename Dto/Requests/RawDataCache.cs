namespace ForecastingGas.Dto.Requests;

public class RawDataCache
{
    public HashSet<string> CacheKeys { get; set; } = new();
}