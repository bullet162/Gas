using ForecastingGas.Data.Entities;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ForecastingGas.Data.Repositories.Implementations;

public class SaveData : ISaveData
{
    private readonly AppDbContext _DB;

    public SaveData(AppDbContext appDbContext)
    {
        _DB = appDbContext;
    }

    //save forecast
    public async Task<bool> SaveDatas(ALgoOutput output)
    {

        var result = new ForecastDescription
        {
            AlgoType = output.AlgoType,
            ColumnName = output.ColumnName,
            TotalCount = output.TotalCount,
            GetForecastValues = output.ForecastValues.Select(x => new ForecastValues
            {
                ForecastValue = x
            }).ToList()
        };

        if (result == null)
            return false;

        else
        {
            await _DB.AddAsync(result);
            await _DB.SaveChangesAsync();

            return true;
        }
    }

    //save actualValues
    public async Task<bool> SaveRawData(RawDataOutput dataOutput)
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

        if (result == null)
            return false;

        else
        {
            await _DB.AddAsync(result);
            await _DB.SaveChangesAsync();

            return true;
        }
    }

    public async Task<bool> SaveErrorData(ErrorOutput output, string algoType, string columnName)
    {
        var results = new ErrorValues
        {
            ColumnName = columnName,
            AlgoType = algoType,
            RMSE = output.RMSE,
            MSE = output.MSE,
            MAE = output.MAE,
            MAPE = output.MAPE
        };

        if (results == null)
            return false;

        else
        {
            await _DB.AddAsync(results);
            await _DB.SaveChangesAsync();

            return true;
        }

    }
}