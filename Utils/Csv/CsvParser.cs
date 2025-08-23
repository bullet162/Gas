using System.Threading.Tasks;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Utils.Csv;

public class UploadCsv : IUploadCsv
{
    public async Task<List<string>> ShowColumnNames(FileUpload fileUpload)
    {
        var fileName = fileUpload.File.FileName;
        var extensionName = Path.GetExtension(fileName);
        if (extensionName != ".csv")
            throw new ArgumentException("File must be a csv.");

        var columnNames = new List<string>();
        using (StreamReader reader = new StreamReader(fileUpload.File.OpenReadStream()))
        {
            var header = await reader.ReadLineAsync();

            if (header == null)
                throw new ArgumentNullException("headers don't exist!");

            columnNames = header!
            .Trim()
            .Split(',')
            .Select(c => c.Trim('"'))
            .ToList();


        }

        return columnNames;
    }

    public async Task<(List<decimal> actualValues, string columnName, int totalCount)> GetActualValues(FileUpload fileUpload, SelectedColumnName selectedColumnName)
    {
        var fileName = fileUpload.File.FileName;
        var extensionName = Path.GetExtension(fileName);
        if (extensionName != ".csv")
            throw new ArgumentException("File must be a csv.");

        var displayName = $"{fileName} - {selectedColumnName.SColumnName}";

        var data = new List<decimal>();

        var rawData = new List<string>();

        using (StreamReader reader = new StreamReader(fileUpload.File.OpenReadStream()))
        {
            var headerLine = await reader.ReadLineAsync();
            var columnNames = headerLine!.Split(',').Select(c => c.Trim('"')).ToList();

            int headerIndex = columnNames.IndexOf(selectedColumnName.SColumnName);
            if (headerIndex == -1)
                throw new ArgumentException("Header not found");

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = line!.Split(',');

                if (headerIndex < values.Length && decimal.TryParse(values[headerIndex], out decimal result))
                {
                    data.Add(result);
                }
                else
                {
                    throw new ArgumentException("Cannot parse the data..");
                }
            }

            return (data, displayName, data.Count);
        }
    }
}