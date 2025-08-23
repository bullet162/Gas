using ForecastingGas.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.CRUD;

[ApiController]
[Route("api/Read")]
public class ReadErrorData : ControllerBase
{
    private readonly IGetError _getError;

    public ReadErrorData(IGetError error)
    {
        _getError = error;
    }

    [HttpGet("ErrorData")]
    public async Task<IActionResult> GetErrorData([FromQuery] int Id)
    {
        try
        {
            var result = await _getError.GetErrorOutputsById(Id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}