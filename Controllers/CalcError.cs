using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Error_Metrics.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/calcError")]
public class CalcError : ControllerBase
{
    private readonly IGetData _getData;
    private ISaveData _saveData;
    private IError _calcError;
    private readonly IGetForecastValues _getForecastValues;

    public CalcError(IGetData getData, ISaveData saveData, IError calcError, IGetForecastValues getForecastValues)
    {
        _getData = getData;
        _saveData = saveData;
        _calcError = calcError;
        _getForecastValues = getForecastValues;
    }

    [HttpPost("AlgorithmEvaluation")]
    public async Task<IActionResult> EvaluateAlgorithms([FromBody] CalcErrors calcError)
    {
        try
        {
            var AlgoType = calcError.AlgoType;
            var headerName = calcError.ColumnName;

            if (string.IsNullOrWhiteSpace(AlgoType))
                return BadRequest("Invalid AlgorithmType!");

            var fDatas = await _getForecastValues.GetForecastValuesByColumnName(headerName);
            var data = await _getData.ActualValues(headerName);

            var algoData = fDatas
            .Where(x => x.AlgoType.Trim().ToLower() == AlgoType.Trim().ToLower())
            .Select(d => new ALgoOutput
            {
                ForecastValues = d.ForecastValues.ToList(),
                AlgoType = d.AlgoType
            });

            if (!algoData.Any() || algoData == null)
                return NotFound("No Data Found!");

            var fdescriptions = await _getForecastValues.GetForecastDescriptions();

            var seasonLength = fdescriptions
            .Where(x => x.ColumnName == headerName && x.AlgoType.ToLower() == "gas".ToLower())
            .Select(x => x.SeasonLength)
            .FirstOrDefault();

            var errorParams = new ErrorParams
            {
                SeasonLength = seasonLength,
                ActualValues = data.Values.ToList(),
                ForecastValues = algoData.SelectMany(x => x.ForecastValues).ToList()
            };

            var error = _calcError.EvaluateAlgoErrors(errorParams);

            var result = new ErrorOutput
            {
                ColumnName = headerName,
                AlgoType = AlgoType,
                RMSE = error.RMSE,
                MAPE = error.MAPE,
                MSE = error.MSE,
                MAE = error.MAE
            };

            await _saveData.SaveErrorData(result);

            return Ok(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Something went wrong {ex.Message}");
        }
    }
}