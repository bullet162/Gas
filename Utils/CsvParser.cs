using ForecastingGas.Dto.Requests;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Utils.Csv;

public class UploadCsv : IUploadCsv
{
    public async Task<List<string>> ShowColumnNames(FileUpload fileUpload)
    {
        var ext = Path.GetExtension(fileUpload.File.FileName);
        if (!ext.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File must be a .csv.");

        using var reader = new StreamReader(fileUpload.File.OpenReadStream());
        var header = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(header))
            throw new ArgumentException("CSV has no header row.");

        return ParseCsvLine(header);
    }

    public async Task<(List<decimal> actualValues, string columnName, int totalCount)> GetActualValues(
        FileUpload fileUpload, SelectedColumnName selectedColumnName)
    {
        var ext = Path.GetExtension(fileUpload.File.FileName);
        if (!ext.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File must be a .csv.");

        using var reader = new StreamReader(fileUpload.File.OpenReadStream());

        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
            throw new ArgumentException("CSV has no header row.");

        var columns = ParseCsvLine(headerLine);
        int colIndex = columns.IndexOf(selectedColumnName.SColumnName.Trim());

        if (colIndex == -1)
            throw new ArgumentException($"Column '{selectedColumnName.SColumnName}' not found. Available: {string.Join(", ", columns)}");

        var data = new List<decimal>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line);
            if (colIndex >= values.Count) continue;

            var raw = values[colIndex].Trim();

            // Skip non-numeric rows (headers repeated, empty, text labels)
            if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal parsed))
            {
                data.Add(parsed);
            }
        }

        if (data.Count == 0)
            throw new ArgumentException("No numeric data found in the selected column.");

        // Cap at 1000 most recent rows
        if (data.Count > 1000)
            data = data.TakeLast(1000).ToList();

        var displayName = $"{fileUpload.File.FileName} - {selectedColumnName.SColumnName}";
        return (data, displayName, data.Count);
    }

    /// <summary>
    /// Splits a CSV line respecting quoted fields (handles commas inside quotes).
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Handle escaped quotes ""
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString().Trim());
        return fields;
    }
}
