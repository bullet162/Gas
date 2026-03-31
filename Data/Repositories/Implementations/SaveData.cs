using ForecastingGas.Data.Entities;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace ForecastingGas.Data.Repositories.Implementations;

public class SaveData : ISaveData
{
    private readonly AppDbContext _DB;
    private readonly IMemoryCache _cache;
    private readonly RawDataCache _CacheKeys;

    public SaveData(AppDbContext appDbContext, IMemoryCache cache, RawDataCache cacheKeys)
    {
        _DB = appDbContext;
        _cache = cache;
        _CacheKeys = cacheKeys;
    }

    public async Task<bool> SaveRawData(RawDataOutput dataOutput)
    {
        string cacheKey = dataOutput.ColumnName.Trim().ToLower();

        try
        {
            var entity = new DataDescription
            {
                ColumnName = dataOutput.ColumnName,
                TotalCount = dataOutput.TotalCount,
                ActualValues = dataOutput.ActualValues.Select(x => new ActualValues
                {
                    ActualValue = x
                }).ToList()
            };

            await _DB.AddAsync(entity);
            await _DB.SaveChangesAsync();

            // Cache after successful DB save with the DB-assigned ID
            dataOutput.Id = entity.Id;
            _cache.Set(cacheKey, dataOutput, TimeSpan.FromMinutes(30));
            _CacheKeys.Add(cacheKey);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
