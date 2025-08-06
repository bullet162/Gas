using ForecastingGas.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.Test;

[ApiController]
[Route("api/test")]
public class Error : ControllerBase
{
    private IDataProvider _data;
    public Error(IDataProvider dataProvider)
    {
        _data = dataProvider;
    }

    [HttpPost("random")]
    public IActionResult GenerateRandomNumbers([FromBody] int count)
    {
        try
        {
            var result = _data.RandomGenerator(count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"may mali dito sigurado: {ex.Message}");
        }
    }
}