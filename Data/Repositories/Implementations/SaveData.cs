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
            GetForecastValues = output.ForecastValues
            .Select(x => new ForecastValues
            {
                ForecastValue = x
            }).ToList(),
            GetPredictionValues = output.PredictionValues
            .Select(x => new PredictionValues
            {
                PredictionValue = x
            }).ToList(),
            GetPredictionValues2 = output.PredictionValues2
            .Select(x => new PredictionValues2
            {
                PredictionValue2 = x
            }).ToList(),
            TimeComputed = output.TimeComputed,
            AlphaSes = (double)output.AlphaSes,
            AlphaHwes = (double)output.AlphaHwes,
            Beta = (double)output.Beta,
            Gamma = (double)output.Gamma

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

    public async Task<bool> SaveErrorData(ErrorOutput output)
    {
        var results = new ErrorValues
        {
            ColumnName = output.ColumnName,
            AlgoType = output.AlgoType,
            RMSE = output.RMSE,
            MSE = output.MSE,
            MAE = output.MAE,
            MAPE = output.MAPE,
            RMSE2 = output.RMSE2,
            MSE2 = output.MSE2,
            MAE2 = output.MAE2,
            MAPE2 = output.MAPE2
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