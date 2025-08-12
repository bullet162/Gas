using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Utils.Interfaces;


public interface IUploadCsv
{
    Task<List<string>> ShowColumnNames(FileUpload fileUpload);
    Task<(List<decimal> actualValues, string columnName, int totalCount)> GetActualValues(FileUpload fileUpload, SelectedColumnName selectedColumnName);
}