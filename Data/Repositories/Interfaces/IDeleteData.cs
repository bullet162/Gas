namespace ForecastingGas.Data.Repositories.Implementations;

public interface IDeleteData
{
    Task<bool> DeleteDataById(int id);
    Task<bool> DeleteAllData();
}