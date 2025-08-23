using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.CRUD;

[ApiController]
[Route("api/Read")]
public class ReadForecast : ControllerBase
{
    private readonly IGetForecastValues _get;

    public ReadForecast(IGetForecastValues get)
    {
        _get = get;
    }

    [HttpGet("ForecastValuesDescriptions")]
    public async Task<IActionResult> GetFColumnNamesAndId()
    {
        try
        {
            var result = await _get.GetForecastDescriptions();

            if (result.Count == 0)
                return NotFound("No forecast descriptions found.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }

    [HttpGet("ForecastValuesByColumnName")]
    public async Task<IActionResult> GetFActualValues([FromQuery] string columnName)
    {
        try
        {
            if (columnName == null)
                return BadRequest("Invalid column name provided.");

            var result = await _get.GetForecastValuesByColumnName(columnName);

            if (result == null || result.Count == 0)
                return NotFound("No records found!");
            else
                return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }
}