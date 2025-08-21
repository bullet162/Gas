namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IDeleteForecast
{
    Task<bool> DeleteForecastById(int id);
    Task<bool> DeleteAllForecasts();
}