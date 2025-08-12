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
    private IMtGas _gas;

    public Tests(IHwes hwes, ISes ses, IError error, IMtGas gas)
    {
        _hwes = hwes;
        _ses = ses;
        _gas = gas;
    }

    [HttpPost("ses")]
    public IActionResult ForecastSes([FromBody] SesParams sesParams)
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
    public IActionResult ForecastHwes([FromBody] HwesParams hwesParams)
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

    //     [HttpPost("MTGas")]
    //     public IActionResult ForecastGas([FromBody] int id)
    //     {
    //         try
    //         {
    //             var results = _gas.ApplyMtGas();

    //             return Ok(new
    //             {
    //                 results.ColumnName,
    //                 results.TotalCount,
    //                 results.AlgoType,
    //                 results.ForecastValues,
    //                 results.ActualValues
    //             });
    //         }
    //         catch (Exception ex)
    //         {
    //             return BadRequest($"Something went wrong: {ex.Message}");
    //         }
    //     }
}