using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace forecastingGas.Controllers.CRUD;

[ApiController]
[Route("api/Read")]
public class ReadActualValues : ControllerBase
{
    private IGetData _get;

    public ReadActualValues(IGetData get)
    {
        _get = get;
    }

    [HttpGet("ActualValuesDescriptions")]
    public async Task<IActionResult> GetColumnNamesAndId()
    {
        try
        {
            var result = await _get.GetAllColumnNamesAndId();

            if (result == null || result.Count == 0)
                return NotFound("No records found.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }

    [HttpGet("ActualValuesByColumnName")]
    public async Task<IActionResult> GetActualValues([FromQuery] string columnName)
    {
        try
        {
            var result = await _get.ActualValues(columnName);

            if (result.Values == null || result.Values.Count == 0 && result.ColumnName == string.Empty)
                return NotFound("No actual values found for the requested Id.");

            return Ok(new
            {
                result.ColumnName,
                result.Values,
                result.Values.Count
            });
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }
}