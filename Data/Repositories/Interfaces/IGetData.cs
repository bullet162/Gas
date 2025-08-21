using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IGetData
{
    Task<(List<decimal> Values, string ColumnName)> ActualValues(string columnName);
    Task<List<RawDataOutput>> GetAllColumnNamesAndId();
}