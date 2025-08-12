using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace forecastingGas.Controllers.CRUD;

[ApiController]
[Route("api/ReadData")]
public class ReadController : ControllerBase
{
    private IGetData _get;

    public ReadController(IGetData get)
    {
        _get = get;
    }

    [HttpPost("getColumnNamesAndId")]
    public async Task<IActionResult> GetColumnNamesAndId()
    {
        try
        {
            var result = await _get.GetAllColumnNamesAndId();

            return Ok(new
            {
                result.Ids,
                result.ColumnNames
            });
        }
        catch (Exception ex)
        {
            return NotFound($"Something went wrong: {ex.Message}");
        }
    }
}