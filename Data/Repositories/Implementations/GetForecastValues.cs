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

    public async Task<ALgoOutput> GetForecastValuesByColumnName(string columnName, bool LogTransformed, string AlgoType)
    {
        var forecast = await _DBContext.GetForecastDescriptions
     .Where(d => d.isLogTransformed == LogTransformed &&
                 d.ColumnName.Trim().ToLower() == columnName.Trim().ToLower() &&
                 d.AlgoType.Trim().ToLower() == AlgoType.Trim().ToLower())
     .FirstOrDefaultAsync();

        if (forecast == null)
            return null!;

        var algoOutput = new ALgoOutput
        {
            AlgoType = forecast.AlgoType,
            ColumnName = forecast.ColumnName!,
            AlphaSes = (decimal)forecast.AlphaSes,
            AlphaHwes = (decimal)forecast.AlphaHwes,
            Beta = (decimal)forecast.Beta,
            Gamma = (decimal)forecast.Gamma,
            Id = forecast.Id,
            IsLogTransformed = forecast.isLogTransformed
        };


        algoOutput.PredictionValues = await _DBContext.GetPredictionValues
            .Where(pv => pv.ForecastDescriptionID2 == forecast.Id)
            .OrderBy(pv => pv.Id)
            .Select(pv => pv.PredictionValue)
            .ToListAsync();


        algoOutput.PredictionValues2 = await _DBContext.GetPredictionValues2
                .Where(pv => pv.ForecastDescriptionID3 == forecast.Id)
                .OrderBy(pv => pv.Id)
                .Select(pv => pv.PredictionValue2)
                .ToListAsync();

        algoOutput.PreditionValuesAverage = await _DBContext.GetPredictionValues3
            .Where(pv => pv.ForecastDescriptionID4 == forecast.Id)
            .OrderBy(pv => pv.Id)
            .Select(pv => pv.PredictionValue3)
            .ToListAsync();

        return algoOutput;
    }
}