using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/algorithm")]
public class ForecastController : ControllerBase
{
    private IMtGas _gas;
    private readonly IGetData _data;
    private ISes _ses;
    private IHwes _hwes;
    private ISearch _search;
    private IProcessing _process;
    public ForecastController
    (IMtGas gas, IGetData data, ISes ses, IHwes hwes,
    ISearch search, IProcessing processing)
    {
        _gas = gas;
        _data = data;
        _ses = ses;
        _hwes = hwes;
        _search = search;
        _process = processing;
    }

    [HttpPost("prediction")]
    public async Task<IActionResult> Predict([FromBody] BenchmarkParams input, [FromQuery] int forecastHorizon)
    {
        try
        {
            string algoType = input.AlgoType.Trim().ToLower();
            bool isLogTransformed = input.LogTransform.Trim().ToLower() == "yes" ? true : false;
            int seasonLength = input.SeasonLength;

            var data = await _data.ActualValues(input.ColumnName);

            var optimalParameters = _search.GridSearchHWES(data.ActualValues, seasonLength);
            var optimalAlpha = _search.GenerateOptimalAlpha(data.ActualValues);

            var hwesParams = new HwesParams
            {
                Alpha = optimalParameters.alpha,
                Beta = optimalParameters.beta,
                Gamma = optimalParameters.gamma,
                ForecasHorizon = forecastHorizon == 0 ? 1 : forecastHorizon,
                SeasonLength = seasonLength,
                ActualValues = data.ActualValues,
                ForecastValues = new List<decimal>(),
                SeasonalValues = new List<decimal>(),
                TrendValues = new List<decimal>(),
                LevelValues = new List<decimal>(),
                PredictionValues = new List<decimal>(),
                AddPrediction = "yes"
            };

            var gasRequest = new GasRequest
            {
                ColumnName = data.ColumnName,
                AlphaSes = optimalAlpha,
                AddPrediction = "yes"
            };

            List<decimal> realData = isLogTransformed == true ? _process.LogTransformation(data.ActualValues) : data.ActualValues;

            ALgoOutput result = new();

            switch (algoType)
            {
                case "ses":
                    result = _ses.SesForecast(optimalAlpha, realData, forecastHorizon == 0 ? 1 : forecastHorizon);
                    break;

                case "hwes":
                    result = _hwes.TrainForecast(hwesParams);
                    break;

                case "gas":
                    result = _gas.ApplyMtGas(hwesParams, gasRequest);
                    break;

                default:
                    throw new Exception("Something went wrong.");
            }

            return Ok(result);

        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}