using ForecastingGas.Data;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Algorithm.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/sesForecast")]
public class ForecastSes : ControllerBase
{
    private readonly IGetData _get;
    private ISes _ses;
    private ISaveData _save;
    public ForecastSes(IGetData getData, ISes ses, ISaveData save)
    {
        _get = getData;
        _ses = ses;
        _save = save;
    }

    //forecast ses
    [HttpPost("ses")]
    public async Task<IActionResult> Forecast([FromBody] InputSesController ses)
    {
        try
        {
            var data = await _get.ActualValues(ses.ColumnName);
            SesParams sesParams = new SesParams
            {
                ActualValues = data.Values,
                Alpha = ses.Alpha
            };
            var result = _ses.SesForecast(sesParams);

            var output = new ALgoOutput
            {
                AlgoType = result.AlgoType,
                ColumnName = data.ColumnName,
                TotalCount = result.TotalCount,
                ForecastValues = result.ForecastValues
            };

            await _save.SaveDatas(output);
            return Ok("Forecasting completed successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong: {ex.Message}");
        }
    }

}