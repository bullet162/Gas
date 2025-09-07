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
public class BenchmarkController : Controller
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
    public BenchmarkController
    (ITrainTest trainTest, IGetData get, ISes ses, IHwes hwes, IMtGas gas,
    IError error, IProcessing processing, ISaveData save, ISearch search,
    IGetForecastValues getF, IGetError getError)
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
    }

    [HttpPost("forecast")]
    public async Task<IActionResult> Forecast([FromBody] BenchmarkParams benchmark)
    {
        try
        {
            var errorOutput = new ErrorOutput();
            var algoOutput = new ALgoOutput();

            if (benchmark.ColumnName == null)
                return BadRequest("Column name of actual values required!");



            var data = await _get.ActualValues(benchmark.ColumnName);

            if (data.ActualValues == null || data.ActualValues.Count == 0)
                return NotFound("No data found with that column name...");

            bool isLogTransformed = false;
            if (benchmark.LogTransform.Trim().ToLower() == "yes")
                isLogTransformed = true;
            else
                isLogTransformed = false;

            var exFData = await _getf.GetForecastValuesByColumnName(benchmark.ColumnName, isLogTransformed, benchmark.AlgoType);

            if (exFData != null
                && exFData.AlgoType.Trim().ToLower() == benchmark.AlgoType.Trim().ToLower()
                && exFData.ColumnName.Trim().ToLower() == benchmark.ColumnName.Trim().ToLower()
                && exFData.IsLogTransformed == isLogTransformed)
            {
                algoOutput = exFData;
                errorOutput = await _getErr.GetErrorOutputsById(exFData.Id);
                return Ok(new { algoOutput, errorOutput });
            }
            else
            {


                List<decimal> LogValues = new();

                if (benchmark.LogTransform.Trim().ToLower() == "yes")
                    LogValues = _process.LogTransformation(data.ActualValues);
                else
                    LogValues = data.ActualValues;

                var ActualValues = _TrainTest.SplitDataTwo(LogValues);

                var error1 = new ErrorOutput();
                var error2 = new ErrorOutput();
                var error3 = new ErrorOutput();

                var seasonLength = ActualValues.Train.Count / 10;

                var optimizedHwes = _search.GridSearchHWES(ActualValues.Train, seasonLength);

                var hwesParams = new HwesParams
                {
                    Alpha = optimizedHwes.alpha,
                    Beta = optimizedHwes.beta,
                    Gamma = optimizedHwes.gamma,
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

                List<decimal> backfData = new();
                List<decimal> backfData2 = new();
                List<decimal> backfData3 = new();

                List<decimal> backData = new();
                var input = benchmark.AlgoType.Trim().ToLower();


                if (input == "ses")
                {
                    var optimize = _search.GenerateOptimalAlpha(ActualValues.Train);
                    result = _ses.SesForecast(optimize, ActualValues.Train, ActualValues.Test.Count);
                }

                else if (input == "hwes")
                    result = _hwes.TrainForecast(hwesParams);

                else if (input == "gas")
                {
                    result = _gas.ApplyMtGas(hwesParams, gasParams);

                    if (benchmark.LogTransform.Trim().ToLower() == "yes")
                    {
                        backfData = _process.BackLogTransform(result.PredictionValues);
                        backfData2 = _process.BackLogTransform(result.PredictionValues2);
                        backfData3 = _process.BackLogTransform(result.PreditionValuesAverage);

                        backData = _process.BackLogTransform(ActualValues.Test);
                    }
                    else
                    {
                        backfData = result.PredictionValues;
                        backfData2 = result.PredictionValues2;
                        backfData3 = result.PreditionValuesAverage;

                        backData = ActualValues.Test;
                    }
                    var errorParam = new ErrorEvaluate
                    {
                        ActualValues = backData,
                        ForecastValues = backfData
                    };

                    var errorParam2 = new ErrorEvaluate
                    {
                        ActualValues = backData,
                        ForecastValues = backfData2
                    };


                    var errorParam3 = new ErrorEvaluate
                    {
                        ActualValues = backData,
                        ForecastValues = backfData3
                    };


                    error1 = _error.EvaluateAlgoErrors(errorParam);
                    error2 = _error.EvaluateAlgoErrors(errorParam2);
                    error3 = _error.EvaluateAlgoErrors(errorParam3);
                }

                else
                    return NotFound("404!");

                if (input == "hwes" || input == "ses" && input != "gas")
                {
                    if (benchmark.LogTransform.Trim().ToLower() == "yes")
                    {
                        backfData = _process.BackLogTransform(result.PredictionValues);
                        backData = _process.BackLogTransform(ActualValues.Test);
                    }
                    else
                    {
                        backfData = result.PredictionValues;
                        backData = ActualValues.Test;
                    }

                    var errorParams = new ErrorEvaluate
                    {
                        ActualValues = backfData,
                        ForecastValues = backData
                    };

                    error1 = _error.EvaluateAlgoErrors(errorParams);
                }

                algoOutput = new ALgoOutput
                {
                    AlgoType = result.AlgoType,
                    ColumnName = data.ColumnName,
                    ForecastValues = backData,
                    PredictionValues = backfData,
                    PredictionValues2 = backfData2,
                    PreditionValuesAverage = backfData3,
                    AlphaSes = result.AlphaSes,
                    AlphaHwes = result.AlphaHwes,
                    Beta = result.Beta,
                    TimeComputed = result.TimeComputed,
                    SeasonLength = result.SeasonLength,
                    DatePredicted = DateTime.Now,
                    IsLogTransformed = isLogTransformed,
                    TotalCount = result.PredictionValues.Count + result.PredictionValues2.Count
                    + result.PreditionValuesAverage.Count
                };
                var entity = await _save.SaveDatas(algoOutput);
                algoOutput.Id = entity.Id;
                errorOutput = new ErrorOutput
                {
                    ForecastID = algoOutput.Id,
                    AlgoType = result.AlgoType,
                    ColumnName = data.ColumnName,
                    isLogTransformed = algoOutput.IsLogTransformed,
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
                    RMSE3 = error3.RMSE,
                };

                await _save.SaveErrorData(errorOutput);

                return Ok(new
                {
                    algoOutput,
                    errorOutput
                });
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong {ex.Message}");
        }
    }

}