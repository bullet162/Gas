namespace ForecastingGas.Data.Repositories.Implementations;

public interface IDeleteData
{
    Task<bool> DeleteDataByColumnName(string columnName);
    Task<bool> DeleteAllData();
}