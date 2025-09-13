using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.CRUD;

[ApiController]
[Route("api/Delete")]
public class DeleteForecastValues : ControllerBase
{
    private IDeleteForecast _delete;
    public DeleteForecastValues(IDeleteForecast delete)
    {
        _delete = delete;
    }
    [HttpDelete("ForecastValuesById")]
    public async Task<IActionResult> DeleteForecastById([FromQuery] int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Invalid Id provided.");

            var result = await _delete.DeleteForecastById(id);

            if (!result)
                return NotFound("No forecast found with the provided Id.");

            return Ok("Forecast deleted successfully.");
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }

    [HttpDelete("AllForecasts")]
    public async Task<IActionResult> DeleteAllForecasts()
    {
        try
        {
            var result = await _delete.DeleteAllForecasts();

            if (!result)
                return NotFound("No forecasts found to delete.");

            return Ok("All forecasts deleted successfully.");
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }
}