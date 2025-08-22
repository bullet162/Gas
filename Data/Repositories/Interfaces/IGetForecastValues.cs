using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IGetForecastValues
{
    Task<List<GetForecast>> GetForecastDescriptions();
    Task<List<ALgoOutput>> GetForecastValuesByColumnName(string columnName);
}