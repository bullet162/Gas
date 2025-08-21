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

    // [HttpPost("calculateErrorByIdOfForecast")]
    // public async Task<IActionResult> CalculateError([FromBody] int Id)
    // {
    //     try
    //     {
    //         var fData = await _getForecastValues.GetForecastValuesById(Id);

    //         var actualData = await _getData.ActualValues(fData.ColumnName);

    //         var ErrorParams = new ErrorParams
    //         {

    //         };

    //     }
    // }
}