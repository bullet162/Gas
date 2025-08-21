using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data.Repositories.Implementations;

public class Data : IGetData
{
    private readonly AppDbContext _DbContext;

    public Data(AppDbContext appDbContext)
    {
        _DbContext = appDbContext;
    }
    //Get all ColumnName and id
    public async Task<List<RawDataOutput>> GetAllColumnNamesAndId()
    {
        var result = await _DbContext.DataDescriptions
        .Select(x => new RawDataOutput
        {
            Id = x.Id,
            ColumnName = x.ColumnName,
            TotalCount = x.TotalCount,
            DateOfEntry = x.DateUploaded,
            ActualValues = new List<decimal>()
        })
        .OrderBy(x => x.DateOfEntry)
        .ThenBy(x => x.Id)
        .ToListAsync();

        return result;
    }

    //Get ColumnName and ActualValues by id
    public async Task<(List<decimal> Values, string ColumnName)> ActualValues(string columnName)
    {

        if (string.IsNullOrEmpty(columnName))
            throw new ArgumentException("Column name cannot be null or empty.");

        var data = await _DbContext.GetActualValues
            .Where(x => x.GetDataDescription.ColumnName == columnName)
            .Select(x => x.ActualValue)
            .ToListAsync();

        if (data == null || !data.Any() || data.Count < 5)
            throw new ArgumentException("Invalid or insufficient data.");

        else
        {
            var NameofColumn = await _DbContext.DataDescriptions
                .Where(d => d.ColumnName == columnName)
                .Select(d => d.ColumnName)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(NameofColumn))
                throw new ArgumentException("Column name not found for the provided Id.");

            return (data, columnName);
        }
    }

}