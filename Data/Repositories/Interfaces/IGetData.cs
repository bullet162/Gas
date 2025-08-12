namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IGetData
{
    Task<(List<decimal> Values, string ColumnName)> ActualValues(int id);
    Task<(List<string> ColumnNames, List<int> Ids)> GetAllColumnNamesAndId();
}