using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Utils.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Error_Metrics.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/test")]
public class Tests : ControllerBase
{
    private IHwes _hwes;
    private ISes _ses;

    public Tests(IHwes hwes, ISes ses, IError error)
    {
        _hwes = hwes;
        _ses = ses;
    }

    [HttpPost("ses")]
    public IActionResult ForecastSes(SesParams sesParams)
    {
        try
        {
            var results = _ses.SesForecast(sesParams);

            var forecast = results.trainedForecast;
            var model = results.model;
            var count = results.totalCount;
            return Ok(new
            {
                forecast,
                model,
                count
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"may mali: {ex.Message}");
        }
    }

    [HttpPost("hwes")]
    public IActionResult ForecastHwes(HwesParams hwesParams)
    {
        try
        {
            var param = new HwesParams
            {
                Alpha = hwesParams.Alpha,
                Beta = hwesParams.Beta,
                Gamma = hwesParams.Gamma,
                ForecasHorizon = hwesParams.ForecasHorizon,
                SeasonLength = hwesParams.SeasonLength,
                ActualValues = hwesParams.ActualValues,
                ForecastValues = hwesParams.ForecastValues,
            };

            var results = _hwes.TrainForecast(param);

            var predict = _hwes.GenerateForecasts(param);

            return Ok(new
            {
                results.trainedForecast,
                results.model,
                results.totalCount,
                predict.Count,
                predict
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }
}