using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ForecastingGas.Data.Repositories.Implementations;

public class DeleteData : IDeleteData
{
    private readonly AppDbContext _Db;
    private readonly IMemoryCache _cache;
    private readonly RawDataCache _cacheKeys;

    public DeleteData(AppDbContext db, IMemoryCache cache, RawDataCache cacheKeys)
    {
        _Db = db;
        _cache = cache;
        _cacheKeys = cacheKeys;
    }

    public async Task<bool> DeleteDataById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.");

        var result = await _Db.GetDataDescriptions.FirstOrDefaultAsync(x => x.Id == id);

        if (result == null)
            return false;

        var key = result.ColumnName.Trim().ToLower();
        _Db.GetDataDescriptions.Remove(result);
        await _Db.SaveChangesAsync();

        _cache.Remove(key);
        _cacheKeys.Remove(key);

        return true;
    }

    public async Task<bool> DeleteAllData()
    {
        var result = await _Db.GetDataDescriptions.ToListAsync();

        if (result == null || result.Count == 0)
            throw new ArgumentNullException("No data found.");

        foreach (var item in result)
        {
            var key = item.ColumnName.Trim().ToLower();
            _cache.Remove(key);
            _cacheKeys.Remove(key);
        }

        _Db.GetDataDescriptions.RemoveRange(result);
        await _Db.SaveChangesAsync();

        return true;
    }
}
