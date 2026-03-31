using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ForecastingGas.Data.Repositories.Implementations;

public class Data : IGetData
{
    private readonly AppDbContext _DbContext;
    private readonly IMemoryCache _cache;
    private readonly RawDataCache _CacheKeys;

    public Data(AppDbContext appDbContext, IMemoryCache cache, RawDataCache rawDataCache)
    {
        _DbContext = appDbContext;
        _cache = cache;
        _CacheKeys = rawDataCache;
    }

    public async Task<List<RawDataOutput>> GetAllColumnNamesAndId()
    {
        // Try cache first
        var list = new List<RawDataOutput>();
        foreach (var key in _CacheKeys.Keys)
        {
            if (_cache.TryGetValue(key, out RawDataOutput? cachedData) && cachedData != null)
                list.Add(cachedData);
        }

        if (list.Count > 0)
            return list;

        // Cache miss — fall back to DB
        var result = await _DbContext.GetDataDescriptions
            .Select(x => new RawDataOutput
            {
                Id = x.Id,
                ColumnName = x.ColumnName,
                TotalCount = x.TotalCount,
                DateOfEntry = x.DateUploaded,
                ActualValues = x.ActualValues.Select(a => a.ActualValue).ToList()
            })
            .OrderBy(x => x.DateOfEntry)
            .ThenBy(x => x.Id)
            .ToListAsync();

        // Repopulate cache from DB results
        foreach (var item in result)
        {
            var key = item.ColumnName.Trim().ToLower();
            _cache.Set(key, item, TimeSpan.FromMinutes(30));
            _CacheKeys.Add(key);
        }

        return result;
    }

    public async Task<RawDataOutput> ActualValues(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            throw new ArgumentNullException(nameof(columnName), "Column name cannot be null or empty.");

        string key = columnName.Trim().ToLower();

        // Try cache first
        if (_CacheKeys.Contains(key) && _cache.TryGetValue(key, out RawDataOutput? cachedData) && cachedData != null)
            return cachedData;

        // Cache miss — query DB
        var data = await _DbContext.GetDataDescriptions
            .Where(x => x.ColumnName.Trim().ToLower() == key)
            .Select(x => new RawDataOutput
            {
                Id = x.Id,
                ActualValues = x.ActualValues.Select(a => a.ActualValue).ToList(),
                ColumnName = x.ColumnName,
                TotalCount = x.TotalCount,
                DateOfEntry = x.DateUploaded
            })
            .OrderByDescending(x => x.DateOfEntry)
            .FirstOrDefaultAsync();

        if (data == null)
            throw new ArgumentException($"No data found for column '{columnName}'.");

        _cache.Set(key, data, TimeSpan.FromMinutes(30));
        _CacheKeys.Add(key);

        return data;
    }
}
