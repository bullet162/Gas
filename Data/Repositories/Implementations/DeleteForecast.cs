using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data.Repositories.Implementations;

public class DeleteForecast : IDeleteForecast
{
    private readonly AppDbContext _DBContext;

    public DeleteForecast(AppDbContext appDb)
    {
        _DBContext = appDb;
    }

    public async Task<bool> DeleteAllForecasts()
    {
        var forecasts = await _DBContext.GetForecastDescriptions.ToListAsync();

        if (forecasts.Count == 0)
            return false;

        _DBContext.GetForecastDescriptions.RemoveRange(forecasts);
        await _DBContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteForecastById(int id)
    {
        var forecast = await _DBContext.GetForecastDescriptions.FindAsync(id);

        if (forecast == null)
            return false;

        _DBContext.GetForecastDescriptions.Remove(forecast);
        await _DBContext.SaveChangesAsync();
        return true;
    }
}