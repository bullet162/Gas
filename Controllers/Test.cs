using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Error_Metrics.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/test")]
public class Test : ControllerBase
{
    private IHwes _hwes;
    private ISes _ses;
    private IError _error;

    public Test(IHwes hwes, ISes ses, IError error)
    {
        _hwes = hwes;
        _ses = ses;
        _error = error;
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
            _hwes.InitializeComponents(hwesParams);

            var param = new HwesParams
            {
                Alpha = hwesParams.Alpha,
                Beta = hwesParams.Beta,
                Gamma = hwesParams.Gamma,
                ForecasHorizon = hwesParams.ForecasHorizon,
                SeasonLength = hwesParams.SeasonLength,
                ActualValues = hwesParams.ActualValues,
                ForecastValues = hwesParams.ForecastValues,
                Level = hwesParams.Level,
                Trend = hwesParams.Trend,
                Seasonal = hwesParams.Seasonal
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

    [HttpPost("errors")]
    public IActionResult CalculateErrors([FromBody] ErrorParams errorParams)
    {
        try
        {
            var results = _error.CalculateErrors(errorParams);

            return Ok(new
            {
                results.Mae,
                results.Mape,
                results.Rmse,
                results.Mse
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong: {ex.Message}");
        }

    }
}