using ForecastingGas.Data.Entities;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;

namespace ForecastingGas.Data.Repositories.Implementations;

public class SaveData : ISaveData
{
    private readonly AppDbContext _DB;
    private readonly IMemoryCache _cache;
    private readonly RawDataCache _CacheKeys;
    public SaveData(AppDbContext appDbContext, IMemoryCache cache, RawDataCache CacheKeys)
    {
        _DB = appDbContext;
        _cache = cache;
        _CacheKeys = CacheKeys;
    }


    //save actualValues
    public async Task<bool> SaveRawData(RawDataOutput dataOutput)
    {

        string cacheKey = $"{dataOutput.ColumnName.Trim().ToLower()}";

        _cache.Set(cacheKey, dataOutput, TimeSpan.FromMinutes(20));
        _CacheKeys.CacheKeys.Add(cacheKey);

        try
        {
            var result = new DataDescription
            {
                ColumnName = dataOutput.ColumnName,
                TotalCount = dataOutput.TotalCount,
                ActualValues = dataOutput.ActualValues.Select(x => new ActualValues
                {
                    ActualValue = x
                }).ToList()
            };

            await _DB.AddAsync(result);
            await _DB.SaveChangesAsync();

            _cache.Remove(cacheKey);

            return true;
        }
        catch
        {
            return false;
        }
    }


}