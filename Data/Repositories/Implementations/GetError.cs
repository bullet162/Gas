using ForecastingGas.Data;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Implementations;

public class GetError : IGetError
{
    private readonly AppDbContext _Db;

    public GetError(AppDbContext appDbContext)
    {
        _Db = appDbContext;
    }

    public async Task<ErrorOutput> GetErrorOutputsById(int Id)
    {
        if (Id <= 0)
            throw new ArgumentException($"{nameof(Id)} is not valid.");

        var errorData = await _Db.GetErrorValues.FindAsync(Id);

        if (errorData == null)
            throw new ArgumentNullException("No Data Found!");

        var result = new ErrorOutput
        {
            AlgoType = errorData.AlgoType,
            ColumnName = errorData.ColumnName,
            MAE = errorData.MAE,
            MSE = errorData.MSE,
            MAPE = errorData.MAPE,
            RMSE = errorData.RMSE
        };

        return result;
    }
}