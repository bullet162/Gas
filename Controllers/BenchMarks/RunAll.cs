using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Error_Metrics.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.Benchmark;

[ApiController]
[Route("api/benchmark")]
public class BenchmarkController : Controller
{
    private ITrainTest _TrainTest;
    private readonly IGetData _get;
    private ISes _ses;
    private IHwes _hwes;
    private IMtGas _gas;
    private IError _error;

    public BenchmarkController(ITrainTest trainTest, IGetData get, ISes ses, IHwes hwes, IMtGas gas, IError error)
    {
        _TrainTest = trainTest;
        _get = get;
        _ses = ses;
        _hwes = hwes;
        _gas = gas;
        _error = error;
    }

    [HttpPost("forecast")]
    public async Task<IActionResult> Forecast([FromBody] BenchmarkParams benchmark)
    {
        try
        {
            if (benchmark.ColumnName == null)
                return BadRequest("Column name of actual values required!");

            var data = await _get.ActualValues(benchmark.ColumnName);

            if (data.Values == null || data.Values.Count == 0)
                return NotFound("No data found with that column name...");

            var ActualValues = _TrainTest.SplitDataTwo(data.Values);

            var seasonLength = (ActualValues.Train.Count / 2) / 2;

            var hwesParams = new HwesParams
            {
                Alpha = 0.01m,
                Beta = 0.01m,
                Gamma = 0.01m,
                SeasonLength = seasonLength,
                ActualValues = ActualValues.Train,
                ForecasHorizon = ActualValues.Test.Count,
                SeasonalValues = new List<decimal>(),
                TrendValues = new List<decimal>(),
                LevelValues = new List<decimal>(),
                PredictionValues = new List<decimal>(),
                AddPrediction = "yes"
            };

            var gasParams = new GasRequest
            {
                ColumnName = data.ColumnName,
                AddPrediction = "yes"
            };


            var result = new ALgoOutput();

            if (benchmark.AlgoType.Trim().ToLower() == "ses")
                result = _ses.SesForecast(0.01m, ActualValues.Train, ActualValues.Test.Count);

            else if (benchmark.AlgoType.Trim().ToLower() == "hwes")
                result = _hwes.TrainForecast(hwesParams, "yes");

            else if (benchmark.AlgoType.Trim().ToLower() == "gas")
                result = _gas.ApplyMtGas(hwesParams, gasParams);

            Console.WriteLine($"Trend: {result.TrendValues.Count}");
            Console.WriteLine($"Level: {result.LevelValues.Count}");
            Console.WriteLine($"Season: {result.SeasonalValues.Count}");
            Console.WriteLine($"Prediction: {result.PredictionValues.Count}");
            Console.WriteLine($"Test: {ActualValues.Test.Count}");

            var errorParams = new ErrorEvaluate
            {
                ActualValues = ActualValues.Test,
                ForecastValues = result.PredictionValues
            };

            var error = _error.EvaluateAlgoErrors(errorParams);

            return Ok(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong {ex.Message}");
        }
    }

}