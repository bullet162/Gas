using ForecastingGas.Data;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Repositories.Implementations;

public class GetForecastValues : IGetForecastValues
{
    private readonly AppDbContext _DBContext;

    public GetForecastValues(AppDbContext appDb)
    {
        _DBContext = appDb;
    }

    //Get AlgoType, ColumnName and id for forecastValuesSelection
    public async Task<List<GetForecast>> GetForecastDescriptions()
    {
        var result = await _DBContext.GetForecastDescriptions
        .Select(x => new GetForecast
        {
            AlgoType = x.AlgoType,
            ColumnName = x.ColumnName,
            Id = x.Id,
            DateForecasted = x.ForecastDate
        })
        .OrderByDescending(x => x.DateForecasted)
        .ThenBy(x => x.Id)
        .ThenBy(x => x.AlgoType)
        .ToListAsync();

        return result;
    }

    public async Task<(List<decimal> Forecast, string ColumnName)> GetForecastValuesById(int id)
    {
        var result = await _DBContext.GetForecastValues
        .Where(x => x.ForecastDescriptionID == id)
        .OrderBy(x => x.Id)
        .Select(x => x.ForecastValue)
        .ToListAsync();

        var columnName = await _DBContext.GetForecastDescriptions
            .Where(d => d.Id == id)
            .Select(d => d.ColumnName)
            .FirstOrDefaultAsync();

        return new(result, columnName ?? string.Empty);
    }

}