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

    public async Task<ALgoOutput> GetForecastValuesByColumnName(string columnName)
    {
        var result = await _DBContext.GetForecastValues
            .Include(x => x.GetForecastDescription)
            .ThenInclude(d => d.GetForecastValues.OrderBy(fv => fv.Id))
            .Include(x => x.GetForecastDescription)
            .ThenInclude(d => d.GetPredictionValues.OrderBy(pv => pv.Id))
            .Include(x => x.GetForecastDescription)
            .ThenInclude(d => d.GetPredictionValues2.OrderBy(cv => cv.Id))
            .FirstOrDefaultAsync(x => x.GetForecastDescription.ColumnName.Trim().ToLower() == columnName.Trim().ToLower());

        if (result == null) return null!;

        var algoOutput = new ALgoOutput
        {
            ForecastValues = result.GetForecastDescription.GetForecastValues
                .OrderBy(x => x.Id)
                .Select(x => x.ForecastValue).ToList(),

            PredictionValues = result.GetForecastDescription.GetPredictionValues
                .OrderBy(x => x.Id)
                .Select(x => x.PredictionValue).ToList(),

            PredictionValues2 = result.GetForecastDescription.GetPredictionValues2
                .OrderBy(x => x.Id)
                .Select(x => x.PredictionValue2).ToList(),

            AlgoType = result.GetForecastDescription.AlgoType,
            SeasonLength = result.GetForecastDescription.SeasonLength,
            DatePredicted = result.GetForecastDescription.ForecastDate,
            ColumnName = result.GetForecastDescription.ColumnName,
            TotalCount = result.GetForecastDescription.TotalCount,
            TimeComputed = result.GetForecastDescription.TimeComputed
        };

        return algoOutput;
    }
}