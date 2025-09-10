// using ForecastingGas.Algorithm.Gas.Interface;
// using ForecastingGas.Algorithm.Interfaces;
// using ForecastingGas.Data.Repositories.Interfaces;
// using ForecastingGas.Dto.Requests;
// using ForecastingGas.Dto.Responses;
// using Microsoft.AspNetCore.Mvc;

// namespace ForecastingGas.Controllers;

// [ApiController]
// [Route("api/forecast")]
// public class GasForecast : ControllerBase
// {
//     private readonly IGetData _getData;
//     private readonly IGetForecastValues _getF;
//     private ISaveData _saveData;
//     private IMtGas _gasForecastAlgorithm;
//     private ITrainTest _traintest;
//     private ISortPredictions _sort;

//     public GasForecast
//     (IGetData getData, ISaveData saveData, IMtGas gasForecastAlgorithm, ITrainTest trainTest, IGetForecastValues getF, ISortPredictions sort)
//     {
//         _getData = getData;
//         _saveData = saveData;
//         _gasForecastAlgorithm = gasForecastAlgorithm;
//         _traintest = trainTest;
//         _getF = getF;
//         _sort = sort;
//     }

//     [HttpPost("gas")]
//     public async Task<IActionResult> Forecast([FromBody] GasRequestController input)
//     {
//         try
//         {
//             var data = await _getData.ActualValues(input.ColumnName);
//             var column = await _getF.GetForecastDescriptions();

//             var check = column
//             .Where(x => x.ColumnName.Trim().ToLower() == data.ColumnName.Trim().ToLower())
//             .Select(x => x.ColumnName)
//             .FirstOrDefault();

//             if (!string.IsNullOrWhiteSpace(check))
//                 return Conflict(new { message = "Forecast for this dataset already exists." });

//             List<decimal> actualData = new();

//             actualData = _traintest.Cut(data.ActualValues);

//             if (actualData == null || actualData.Count == 0)
//                 return BadRequest("No actual values found for the requested Id.");

//             var remain = data.ActualValues.Skip(actualData.Count).ToList();

//             var hwesParameters = new HwesParams
//             {
//                 ActualValues = data.ActualValues,
//                 Alpha = new decimal(),
//                 Beta = new decimal(),
//                 Gamma = new decimal(),
//                 ForecasHorizon = remain.Count,
//                 ForecastValues = new List<decimal>(),
//                 LevelValues = new List<decimal>(),
//                 TrendValues = new List<decimal>(),
//                 SeasonalValues = new List<decimal>(),
//                 SeasonLength = new int(),
//                 PredictionValues = new List<decimal>(),
//                 AddPrediction = "no"
//             };

//             var gasParameters = new GasRequest
//             {
//                 ColumnName = data.ColumnName,
//                 AddPrediction = "yes"
//             };

//             var result = _gasForecastAlgorithm.ApplyMtGas(hwesParameters, gasParameters);

//             var saveResult = new ALgoOutput
//             {
//                 AlgoType = result.AlgoType,
//                 ColumnName = gasParameters.ColumnName,
//                 TotalCount = result.TotalCount,
//                 ForecastValues = result.ForecastValues,
//                 SeasonLength = result.SeasonLength,
//                 PredictionValues = result.PredictionValues,
//                 PredictionValues2 = result.PredictionValues2,
//                 TimeComputed = result.TimeComputed,
//                 AlphaSes = result.AlphaSes,
//                 AlphaHwes = result.AlphaHwes,
//                 Beta = result.Beta,
//                 Gamma = result.Gamma
//             };

//             // await _saveData.SaveDatas(saveResult);
//             var unsortedPredictions = new PredictionsResult
//             {
//                 MinPrediction = result.PredictionValues,
//                 MaxPrediction = result.PredictionValues2
//             };

//             var sortIt = _sort.SortForecast(unsortedPredictions);


//             return Ok(new
//             {
//                 sortIt,
//                 prediction1 = sortIt.MinPrediction.Count,
//                 prediction2 = sortIt.MaxPrediction.Count,
//                 prediction3 = sortIt.AveragePrediction.Count
//             });
//         }
//         catch (Exception ex)
//         {
//             return BadRequest($"Error occurred: {ex.Message}");
//         }
//     }
// }