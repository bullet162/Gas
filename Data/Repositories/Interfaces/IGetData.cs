namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IGetData
{
    Task<(List<decimal> Values, string ColumnName)> ActualValues(int id);
}