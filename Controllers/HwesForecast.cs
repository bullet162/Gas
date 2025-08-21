using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.AspNetCore.Mvc;
namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/hwesForecast")]
public class HwesForecast : ControllerBase
{
    private readonly IGetData _get;
    private IHwes _hwes;
    private ISaveData _save;

    public HwesForecast(IGetData getData, IHwes hwes, ISaveData save)
    {
        _get = getData;
        _hwes = hwes;
        _save = save;
    }

    [HttpPost("hwes")]
    public async Task<IActionResult> Forecast([FromBody] InputHwesController hwesParams)
    {
        try
        {
            var data = await _get.ActualValues(hwesParams.ColumnName);
            var parametersHwes = new HwesParams
            {
                ActualValues = data.Values,
                SeasonLength = hwesParams.SeasonLength,
                Alpha = hwesParams.Alpha,
                Beta = hwesParams.Beta,
                Gamma = hwesParams.Gamma,
                LevelValues = new List<decimal>(),
                TrendValues = new List<decimal>(),
                SeasonalValues = new List<decimal>(),
                ForecastValues = new List<decimal>(),
                ForecasHorizon = hwesParams.ForecasHorizon
            };

            var result = _hwes.TrainForecast(parametersHwes);
            var seasonCount = result.SeasonalValues.Count;
            var trendCount = result.TrendValues.Count;
            var levelCount = result.LevelValues.Count;

            var saveResult = new ALgoOutput
            {
                AlgoType = result.AlgoType,
                ColumnName = data.ColumnName,
                TotalCount = result.ForecastValues.Count,
                ForecastValues = result.ForecastValues
            };

            await _save.SaveDatas(saveResult);

            return Ok("Forecasting completed successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong: {ex.Message}");
        }
    }

}