using ForecastingGas.Data;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Algorithm.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/Forecast")]
public class SesForecast : ControllerBase
{
    private readonly IGetData _get;
    private ISes _ses;
    private ISaveData _save;
    private ISearch _search;
    public SesForecast(IGetData getData, ISes ses, ISaveData save, ISearch search)
    {
        _get = getData;
        _ses = ses;
        _save = save;
        _search = search;
    }

    //forecast ses
    [HttpPost("ses")]
    public async Task<IActionResult> Forecast([FromBody] InputSesController ses)
    {
        try
        {
            var data = await _get.ActualValues(ses.ColumnName);

            var alpha = _search.GenerateOptimalAlpha(data.Values);

            var result = _ses.SesForecast(alpha, data.Values);

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