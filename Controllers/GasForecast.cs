using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/forecast")]
public class GasForecast : ControllerBase
{
    private readonly IGetData _getData;
    private ISaveData _saveData;
    private IMtGas _gasForecastAlgorithm;
    private ITrainTest _traintest;

    public GasForecast(IGetData getData, ISaveData saveData, IMtGas gasForecastAlgorithm, ITrainTest trainTest)
    {
        _getData = getData;
        _saveData = saveData;
        _gasForecastAlgorithm = gasForecastAlgorithm;
        _traintest = trainTest;
    }

    [HttpPost("gas")]
    public async Task<IActionResult> Forecast([FromBody] GasRequestController input)
    {
        try
        {
            var data = await _getData.ActualValues(input.ColumnName);

            var inputLog = input.LogTransform.Trim().ToLower();

            List<decimal> actualData = new();

            if (inputLog == "yes")
                actualData = _traintest.Cut(data.Values);
            else
                actualData = data.Values;

            if (actualData == null || actualData.Count == 0)
                return BadRequest("No actual values found for the requested Id.");

            var hwesParameters = new HwesParams
            {
                ActualValues = data.Values,
                Alpha = new decimal(),
                Beta = new decimal(),
                Gamma = new decimal(),
                ForecasHorizon = data.Values.Count * (int)0.25,
                ForecastValues = new List<decimal>(),
                LevelValues = new List<decimal>(),
                TrendValues = new List<decimal>(),
                SeasonalValues = new List<decimal>(),
                SeasonLength = new int(),
                PredictionValues = new List<decimal>(),
                AddPrediction = "no"
            };

            var gasParameters = new GasRequest
            {
                ColumnName = data.ColumnName,
                AddPrediction = input.AddPrediction
            };

            var result = _gasForecastAlgorithm.ApplyMtGas(hwesParameters, gasParameters);

            var saveResult = new ALgoOutput
            {
                AlgoType = result.AlgoType,
                ColumnName = gasParameters.ColumnName,
                TotalCount = result.TotalCount,
                ForecastValues = result.ForecastValues,
                SeasonLength = result.SeasonLength,
                PredictionValues = result.PredictionValues,
                PredictionValues2 = result.PredictionValues2,
                TimeComputed = result.TimeComputed,
                AlphaSes = result.AlphaSes,
                AlphaHwes = result.AlphaHwes,
                Beta = result.Beta,
                Gamma = result.Gamma
            };

            await _saveData.SaveDatas(saveResult);

            return Ok(new
            {
                result.PredictionValues,
                result.PredictionValues2,
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error occurred: {ex.Message}");
        }
    }
}