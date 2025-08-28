using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/Forecast")]
public class GasForecast : ControllerBase
{
    private readonly IGetData _getData;
    private ISaveData _saveData;
    private IMtGas _gasForecastAlgorithm;

    public GasForecast(IGetData getData, ISaveData saveData, IMtGas gasForecastAlgorithm)
    {
        _getData = getData;
        _saveData = saveData;
        _gasForecastAlgorithm = gasForecastAlgorithm;
    }

    [HttpPost("gas")]
    public async Task<IActionResult> Forecast([FromBody] GasRequestController input)
    {
        try
        {
            var data = await _getData.ActualValues(input.ColumnName);

            if (data.Values == null || data.Values.Count == 0)
                return BadRequest("No actual values found for the requested Id.");

            var hwesParameters = new HwesParams
            {
                ActualValues = data.Values,
                Alpha = new decimal(),
                Beta = new decimal(),
                Gamma = new decimal(),
                ForecasHorizon = input.ForecasHorizon,
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
                LevelValues = result.LevelValues,
                TrendValues = result.TrendValues,
                SeasonalValues = result.SeasonalValues,
                SeasonLength = result.SeasonLength,
                PredictionValues = result.PredictionValues,
                PredictionValues2 = result.PredictionValues2,
                TimeComputed = result.TimeComputed
            };

            await _saveData.SaveDatas(saveResult);

            return Ok("Forecasting completed successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error occurred: {ex.Message}");
        }
    }
}