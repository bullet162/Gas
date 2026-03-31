using System.Collections.Concurrent;

namespace ForecastingGas.Dto.Requests;

public class RawDataCache
{
    // Thread-safe key tracker — multiple requests can upload simultaneously
    public ConcurrentDictionary<string, byte> CacheKeys { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void Add(string key) => CacheKeys.TryAdd(key, 0);
    public bool Contains(string key) => CacheKeys.ContainsKey(key);
    public void Remove(string key) => CacheKeys.TryRemove(key, out _);
    public IEnumerable<string> Keys => CacheKeys.Keys;
}
