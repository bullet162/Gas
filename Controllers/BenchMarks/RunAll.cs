using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Entities;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Error_Metrics.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.Benchmark;

[ApiController]
[Route("api/algorithm")]
public class BenchmarkController : ControllerBase
{
    private readonly IGetData _get;
    private readonly IGetForecastValues _getf;
    private readonly IGetError _getErr;
    private ISes _ses;
    private IHwes _hwes;
    private IMtGas _gas;
    private IError _error;
    private IProcessing _process;
    private ISaveData _save;
    private ITrainTest _TrainTest;
    private ISearch _search;
    private readonly ILogger<BenchmarkController> _logger;
    public BenchmarkController
    (ITrainTest trainTest, IGetData get, ISes ses, IHwes hwes, IMtGas gas,
    IError error, IProcessing processing, ISaveData save, ISearch search,
    IGetForecastValues getF, IGetError getError, ILogger<BenchmarkController> logger)
    {
        _TrainTest = trainTest;
        _get = get;
        _ses = ses;
        _hwes = hwes;
        _gas = gas;
        _error = error;
        _process = processing;
        _save = save;
        _search = search;
        _getf = getF;
        _getErr = getError;
        _logger = logger;
    }

    [HttpPost("forecast")]
    public async Task<IActionResult> Forecast([FromBody] BenchmarkParams benchmark)
    {
        try
        {
            benchmark.SeasonLength = benchmark.SeasonLength == 0 ? 1 : benchmark.SeasonLength;

            if (string.IsNullOrEmpty(benchmark.ColumnName))
                return BadRequest("Column name of actual values required!");

            _logger.LogInformation($"Column name: {benchmark.ColumnName}");

            var cachedForecast = await _getf.GetForecastValuesByColumnName(
                benchmark.ColumnName,
                benchmark.LogTransform?.Trim().ToLower() == "yes",
                benchmark.AlgoType
            );

            if (cachedForecast != null)
            {
                var cachedError = await _getErr.GetErrorOutputsById(cachedForecast.Id);
                return Ok(new { algoOutput = cachedForecast, errorOutput = cachedError });
            }

            var data = await _get.ActualValues(benchmark.ColumnName);
            if (data?.ActualValues == null || !data.ActualValues.Any())
                return NotFound("No data found with that column name...");

            bool isLogTransformed = benchmark.LogTransform?.Trim().ToLower() == "yes";
            var actualValues = isLogTransformed ? _process.LogTransformation(data.ActualValues) : data.ActualValues;

            var splitData = _TrainTest.SplitDataTwo(actualValues);
            _logger.LogInformation($"Training Data: {splitData.Train.Count}, Test Data: {splitData.Test.Count}");

            ALgoOutput result;
            ErrorOutput error1 = new(), error2 = new(), error3 = new();

            switch (benchmark.AlgoType.Trim().ToLower())
            {
                case "ses":
                    benchmark.SeasonLength = 1;
                    var alpha = _search.GenerateOptimalAlpha(splitData.Train);
                    result = _ses.SesForecast(alpha, splitData.Train, splitData.Test.Count);
                    break;

                case "hwes":
                    var optimizedHwes = _search.GridSearchHWES(splitData.Train, benchmark.SeasonLength);
                    var hwesParams = new HwesParams
                    {
                        Alpha = optimizedHwes.alpha,
                        Beta = optimizedHwes.beta,
                        Gamma = optimizedHwes.gamma,
                        SeasonLength = benchmark.SeasonLength,
                        ActualValues = splitData.Train,
                        ForecasHorizon = splitData.Test.Count
                    };
                    result = _hwes.TrainForecast(hwesParams);
                    break;

                case "gas":
                    var gasParams = new GasRequest { ColumnName = data.ColumnName, AddPrediction = "yes" };
                    var hwesParamsForGas = new HwesParams
                    {
                        Alpha = 0,
                        Beta = 0,
                        Gamma = 0,
                        SeasonLength = benchmark.SeasonLength,
                        ActualValues = splitData.Train,
                        ForecasHorizon = splitData.Test.Count
                    };
                    result = _gas.ApplyMtGas(hwesParamsForGas, gasParams);

                    // Back-transform if log
                    List<decimal> backTest = isLogTransformed ? _process.BackLogTransform(splitData.Test) : splitData.Test;
                    List<decimal> pred1 = isLogTransformed ? _process.BackLogTransform(result.PredictionValues) : result.PredictionValues;
                    List<decimal> pred2 = isLogTransformed ? _process.BackLogTransform(result.PredictionValues2) : result.PredictionValues2;
                    List<decimal> predAvg = isLogTransformed ? _process.BackLogTransform(result.PreditionValuesAverage) : result.PreditionValuesAverage;

                    error1 = _error.EvaluateAlgoErrors(new ErrorEvaluate { ActualValues = backTest, ForecastValues = pred1 });
                    error2 = _error.EvaluateAlgoErrors(new ErrorEvaluate { ActualValues = backTest, ForecastValues = pred2 });
                    error3 = _error.EvaluateAlgoErrors(new ErrorEvaluate { ActualValues = backTest, ForecastValues = predAvg });
                    break;

                default:
                    return NotFound("Invalid AlgoType");
            }

            var algoOutput = new ALgoOutput
            {
                AlgoType = result.AlgoType,
                ColumnName = data.ColumnName,
                ForecastValues = splitData.Test,
                PredictionValues = result.PredictionValues,
                PredictionValues2 = result.PredictionValues2,
                PreditionValuesAverage = result.PreditionValuesAverage,
                AlphaSes = result.AlphaSes,
                AlphaHwes = result.AlphaHwes,
                Beta = result.Beta,
                Gamma = result.Gamma,
                TimeComputed = result.TimeComputed,
                SeasonLength = result.SeasonLength,
                DatePredicted = DateTime.Now,
                IsLogTransformed = isLogTransformed,
                TotalCount = result.PredictionValues.Count + result.PredictionValues2.Count + result.PreditionValuesAverage.Count
            };

            var errorOutput = new ErrorOutput
            {
                ForecastID = algoOutput.Id,
                AlgoType = result.AlgoType,
                ColumnName = data.ColumnName,
                isLogTransformed = isLogTransformed,
                MAE = error1.MAE,
                MSE = error1.MSE,
                MAPE = error1.MAPE,
                RMSE = error1.RMSE,
                MAE2 = error2.MAE,
                MSE2 = error2.MSE,
                MAPE2 = error2.MAPE,
                RMSE2 = error2.RMSE,
                MAE3 = error3.MAE,
                MSE3 = error3.MSE,
                MAPE3 = error3.MAPE,
                RMSE3 = error3.RMSE
            };

            // Optional: save to database/cache

            return Ok(new { algoOutput, errorOutput });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running forecast");
            return BadRequest(new { response = ex.Message });
        }
    }

}