using ForecastingGas.Data;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
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
            DateForecasted = x.ForecastDate,
            SeasonLength = x.SeasonLength
        })
        .OrderByDescending(x => x.DateForecasted)
        .ThenBy(x => x.Id)
        .ThenBy(x => x.AlgoType)
        .ToListAsync();

        return result;
    }

    public async Task<List<ALgoOutput>> GetForecastValuesByColumnName(string columnName)
    {
        var flatResult = await _DBContext.GetForecastValues
            .Where(x => x.GetForecastDescription.ColumnName == columnName)
            .GroupBy(x => new
            {
                x.ForecastDescriptionID,
                x.GetForecastDescription.AlgoType,
                x.GetForecastDescription.Id,
                x.GetForecastDescription.TotalCount
            })
            .Select(g => new ALgoOutput
            {
                Id = g.Key.ForecastDescriptionID,
                AlgoType = g.Key.AlgoType,
                ForecastValues = g.Select(f => f.ForecastValue).ToList(),
                TotalCount = g.Key.TotalCount
            })
            .ToListAsync();

        return flatResult;
    }

}