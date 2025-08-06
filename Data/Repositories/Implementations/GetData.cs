using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data.Repositories.Implementations;

public class Data : IGetData
{
    private readonly AppDbContext _DbContext;

    public Data(AppDbContext appDbContext)
    {
        _DbContext = appDbContext;
    }


    //Get ColumnName and ActualValues by id
    public async Task<(List<decimal> Values, string ColumnName)> ActualValues(int id)
    {
        var data = await _DbContext.GetActualValues
            .Where(x => x.DataDescriptionID == id)
            .Select(x => x.ActualValue)
            .ToListAsync();

        if (data == null || !data.Any() || data.Count < 5)
            throw new ArgumentException("Invalid or insufficient data.");

        else
        {
            var columnName = await _DbContext.DataDescriptions
                .Where(d => d.Id == id)
                .Select(d => d.ColumnName)
                .FirstOrDefaultAsync();

            return (data, columnName ?? string.Empty);
        }
    }

}