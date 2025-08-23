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
    public UploadFile(IUploadCsv upload, ISaveData save)
    {
        _csv = upload;
        _save = save;
    }

    //search for libraries for api callings
    [HttpPost("csvColumnNames")]
    public async Task<IActionResult> UploadCsvforColumnNames([FromForm] FileUpload fileUpload)
    {
        try
        {
            var result = await _csv.ShowColumnNames(fileUpload);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
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

            await _save.SaveRawData(saveResult);

            return Ok(saveResult);
        }
        catch (Exception ex)
        {
            return BadRequest($"something went wrong: {ex.Message}");
        }
    }

}