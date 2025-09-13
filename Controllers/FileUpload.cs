using ForecastingGas.Dto.Responses;
using ForecastingGas.Dto.Requests;
using Microsoft.AspNetCore.Mvc;
using ForecastingGas.Utils.Interfaces;
using ForecastingGas.Data.Repositories.Interfaces;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadFile : ControllerBase
{
    private IUploadCsv _csv;
    private ISaveData _save;
    private readonly IGetData _get;
    public UploadFile(IUploadCsv upload, ISaveData save, IGetData get)
    {
        _csv = upload;
        _save = save;
        _get = get;
    }

    [HttpPost("csvColumnNames")]
    public async Task<IActionResult> UploadCsvforColumnNames([FromForm] FileUpload fileUpload)
    {
        try
        {
            var result = await _csv.ShowColumnNames(fileUpload);

            return Ok(new
            {
                columnNames = result
            });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Something went wrong: {ex.Message}" });
        }
    }

    [HttpPost("csvActualValues")]
    public async Task<IActionResult> UploadCsvforActualDataSave([FromForm] FileUpload fileUpload, [FromForm] SelectedColumnName selectedColumnName)
    {
        try
        {
            var result = await _csv.GetActualValues(fileUpload, selectedColumnName);

            var saveResult = new RawDataOutput
            {
                ColumnName = result.columnName,
                TotalCount = result.totalCount,
                ActualValues = result.actualValues,
                DateOfEntry = DateTime.Today
            };

            var column = await _get.GetAllColumnNamesAndId();

            var check = column
            .Where(x => x.ColumnName.Trim().ToLower() == result.columnName.Trim().ToLower())
            .Select(x => x.ColumnName)
            .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(check))
                return Conflict(new { message = "Data already exist in the database." });

            await _save.SaveRawData(saveResult);

            return Ok(new
            {
                columnName = result.columnName,
                totalCount = result.totalCount,
                actualData = result.actualValues,
                dateOfEntry = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong: {ex.Message}");
        }
    }

}