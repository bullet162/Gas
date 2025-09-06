using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IGetForecastValues
{
    Task<List<GetForecast>> GetForecastDescriptions();
    Task<ALgoOutput> GetForecastValuesByColumnName(string columnName, bool isLogTransformed);
}