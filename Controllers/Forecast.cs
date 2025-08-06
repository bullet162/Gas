using ForecastingGas.Data;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Algorithm.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/forecast")]
public class Forecast : ControllerBase
{
    private IGetData _get;
    private ISes _ses;
    private IHwes _hwes;
    private ISaveData _save;
    public Forecast(IGetData getData, ISes ses, ISaveData save, IHwes hwes)
    {
        _get = getData;
        _ses = ses;
        _save = save;
        _hwes = hwes;
    }

    //forecast ses
    [HttpPost("ses")]
    public async Task<IActionResult> ForecastSes([FromBody] InputSesParams ses)
    {
        try
        {
            var data = await _get.ActualValues(ses.Id);
            SesParams sesParams = new SesParams
            {
                ActualValues = data.Values,
                Alpha = ses.Alpha
            };
            var result = _ses.SesForecast(sesParams);

            var output = new ALgoOutput
            {
                AlgoType = result.Item2,
                ColumnName = data.ColumnName,
                TotalCount = result.Item3,
                ForecastValues = result.Item1
            };

            // await _save.SaveDatas(output);
            return Ok(result.Item1);
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong: {ex.Message}");
        }
    }


}