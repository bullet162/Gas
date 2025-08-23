using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data.Repositories.Implementations;

public class DeleteData : IDeleteData
{
    private readonly AppDbContext _Db;

    public DeleteData(AppDbContext db)
    {
        _Db = db;
    }

    public async Task<bool> DeleteDataByColumnName(string columnName)
    {

        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentNullException("Input is required!");

        var result = await _Db.GetDataDescriptions.FindAsync(columnName);

        if (result == null)
            return false;

        else
        {
            _Db.GetDataDescriptions.Remove(result);
            await _Db.SaveChangesAsync();
            return true;
        }

    }

    public async Task<bool> DeleteAllData()
    {
        var result = await _Db.GetDataDescriptions.ToListAsync();

        if (result == null)
            throw new ArgumentNullException("No Data found!");

        _Db.GetDataDescriptions.RemoveRange(result);
        await _Db.SaveChangesAsync();

        return true;
    }
}