using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/Forecast")]
public class HwesForecast : ControllerBase
{
    private readonly IGetData _get;
    private IHwes _hwes;
    private ISaveData _save;
    private ISearch _search;
    private IWatch _watch;
    public HwesForecast(IGetData getData, IHwes hwes, ISaveData save, ISearch search, IWatch watch)
    {
        _get = getData;
        _hwes = hwes;
        _save = save;
        _search = search;
        _watch = watch;
    }

    [HttpPost("hwes")]
    public async Task<IActionResult> Forecast([FromBody] InputHwesController hwesParams)
    {
        try
        {
            var data = await _get.ActualValues(hwesParams.ColumnName);
            _watch.StartWatch();
            var parameters = _search.GridSearchHWES(_hwes, data.Values, hwesParams.SeasonLength);
            var parametersHwes = new HwesParams
            {
                ActualValues = data.Values,
                SeasonLength = hwesParams.SeasonLength,
                Alpha = parameters.alpha,
                Beta = parameters.beta,
                Gamma = parameters.gamma,
                LevelValues = new List<decimal>(),
                TrendValues = new List<decimal>(),
                SeasonalValues = new List<decimal>(),
                ForecastValues = new List<decimal>(),
                ForecasHorizon = hwesParams.ForecasHorizon
            };
            var result = _hwes.TrainForecast(parametersHwes, hwesParams.AddPrediction);
            var seasonCount = result.SeasonalValues.Count;
            var trendCount = result.TrendValues.Count;
            var levelCount = result.LevelValues.Count;
            var timeComputed = _watch.StopWatch();

            var saveResult = new ALgoOutput
            {
                AlgoType = result.AlgoType,
                ColumnName = data.ColumnName,
                TotalCount = result.ForecastValues.Count,
                ForecastValues = result.ForecastValues,
                LevelValues = result.LevelValues,
                TrendValues = result.TrendValues,
                SeasonalValues = result.SeasonalValues,
                SeasonLength = result.SeasonLength,
                PredictionValues = result.PredictionValues,
                TimeComputed = timeComputed
            };

            await _save.SaveDatas(saveResult);

            return Ok($"Forecasting completed successfully. {timeComputed}s");
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong: {ex.Message}");
        }
    }

}