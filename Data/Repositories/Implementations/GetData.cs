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
        var result = await _DbContext.GetDataDescriptions
        .Select(x => new RawDataOutput
        {
            Id = x.Id,
            ColumnName = x.ColumnName,
            TotalCount = x.TotalCount,
            DateOfEntry = x.DateUploaded,
            ActualValues = x.ActualValues.Select(x => x.ActualValue).ToList()
        })
        .OrderBy(x => x.DateOfEntry)
        .ThenBy(x => x.Id)
        .ToListAsync();

        if (result == null || !result.Any())
            return new List<RawDataOutput>();

        return result;
    }

    //Get ColumnName and ActualValues by columnName
    public async Task<RawDataOutput> ActualValues(string columnName)
    {

        if (string.IsNullOrEmpty(columnName))
            throw new ArgumentNullException("Column name cannot be null or empty.");

        var data = await _DbContext.GetActualValues
            .Where(x => x.GetDataDescription.ColumnName.Trim().ToLower() == columnName.Trim().ToLower())
            .Select(x => new RawDataOutput
            {
                Id = x.GetDataDescription.Id,
                ActualValues = x.GetDataDescription.ActualValues.Select(a => a.ActualValue).ToList(),
                ColumnName = x.GetDataDescription.ColumnName,
                TotalCount = x.GetDataDescription.TotalCount,
                DateOfEntry = x.GetDataDescription.DateUploaded
            })
            .OrderByDescending(x => x.DateOfEntry)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (data == null)
            throw new ArgumentException("Invalid or insufficient data.");

        return data;

    }

}