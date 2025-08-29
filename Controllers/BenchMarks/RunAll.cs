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
    private ISes _ses;
    private IHwes _hwes;
    private IMtGas _gas;
    private IError _error;
    private IProcessing _process;
    private ISaveData _save;
    private ITrainTest _TrainTest;
    private ISearch _search;
    public BenchmarkController
    (ITrainTest trainTest, IGetData get, ISes ses, IHwes hwes, IMtGas gas, IError error, IProcessing processing, ISaveData save, ISearch search)
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

            List<decimal> LogValues = new();

            if (benchmark.LogTransform.Trim().ToLower() == "yes")
                LogValues = _process.LogTransformation(data.Values);
            else
                LogValues = data.Values;

            var ActualValues = _TrainTest.SplitDataTwo(LogValues);

            var error1 = new ErrorOutput();
            var error2 = new ErrorOutput();

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
            var input = benchmark.AlgoType.Trim().ToLower();


            if (input == "ses")
            {
                var optimize = _search.GenerateOptimalAlpha(ActualValues.Train);
                result = _ses.SesForecast(optimize, ActualValues.Train, ActualValues.Test.Count);
            }

            else if (input == "hwes")
                result = _hwes.TrainForecast(hwesParams);

            else if (input == "oldgas")
            {
                result = _gas.ApplyMtGas(hwesParams, gasParams);

                var errorParam = new ErrorEvaluate
                {
                    ActualValues = ActualValues.Test,
                    ForecastValues = result.PredictionValues
                };

                var errorParam2 = new ErrorEvaluate
                {
                    ActualValues = ActualValues.Test,
                    ForecastValues = result.PredictionValues2
                };

                error1 = _error.EvaluateAlgoErrors(errorParam);
                error2 = _error.EvaluateAlgoErrors(errorParam2);
            }

            else
                return NotFound("404!");

            var errorParams = new ErrorEvaluate
            {
                ActualValues = ActualValues.Test,
                ForecastValues = result.PredictionValues
            };

            error1 = _error.EvaluateAlgoErrors(errorParams);

            List<decimal> Pred1 = new();
            List<decimal> Pred2 = new();
            List<decimal> data1 = new();

            if (benchmark.LogTransform.Trim().ToLower() == "yes")
            {
                Pred1 = _process.BackLogTransform(result.PredictionValues);
                if (input == "oldgas")
                    Pred2 = _process.BackLogTransform(result.PredictionValues2);
                data1 = _process.BackLogTransform(ActualValues.Test);
            }
            else
            {
                Pred1 = result.PredictionValues;
                if (input == "oldgas")
                    Pred2 = result.PredictionValues2;
                data1 = ActualValues.Test;
            }

            var algoOutput = new ALgoOutput
            {
                AlgoType = result.AlgoType,
                ColumnName = data.ColumnName,
                PredictionValues = Pred1,
                PredictionValues2 = Pred2,
                AlphaSes = result.AlphaSes,
                AlphaHwes = result.AlphaHwes,
                Beta = result.Beta,
                TimeComputed = result.TimeComputed,
                SeasonLength = result.SeasonLength
            };

            var errorOutput = new ErrorOutput
            {
                AlgoType = result.AlgoType,
                ColumnName = data.ColumnName,
                MAE = error1.MAE,
                MSE = error1.MSE,
                MAPE = error1.MAPE,
                RMSE = error1.RMSE,
                MAE2 = error2.MAE,
                MSE2 = error2.MSE,
                MAPE2 = error2.MAPE,
                RMSE2 = error2.RMSE
            };

            await _save.SaveDatas(algoOutput);
            await _save.SaveErrorData(errorOutput);

            return Ok(new
            {
                result.AlgoType,
                data.ColumnName,
                result.AlphaSes,
                result.AlphaHwes,
                result.Beta,
                result.Gamma,
                data1,
                Pred1,
                Pred2,
                error1,
                error2,
                result.TimeComputed
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong {ex.Message}");
        }
    }

}