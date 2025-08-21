using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IGetForecastValues
{
    Task<List<GetForecast>> GetForecastDescriptions();
    Task<(List<decimal> Forecast, string ColumnName)> GetForecastValuesById(int id);
}