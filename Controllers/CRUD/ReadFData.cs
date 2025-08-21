using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.CRUD;

[ApiController]
[Route("api/ReadForecastData")]
public class ReadFController : ControllerBase
{
    private readonly IGetForecastValues _get;

    public ReadFController(IGetForecastValues get)
    {
        _get = get;
    }

    [HttpGet("getFColumnNamesAndId")]
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

    [HttpGet("getFActualValues")]
    public async Task<IActionResult> GetFActualValues([FromQuery] int Id)
    {
        try
        {
            if (Id <= 0)
                return BadRequest("Invalid Id provided.");

            var result = await _get.GetForecastValuesById(Id);

            if (result.Forecast.Count == 0)
                return NotFound("No forecast values found for the requested Id.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }
}