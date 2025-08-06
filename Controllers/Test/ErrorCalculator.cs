using ForecastingGas.Dto.Requests;
using ForecastingGas.Error_Metrics.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.Test;

[ApiController]
[Route("api/test")]
public class RandomN : ControllerBase
{
    private IError _error;

    public RandomN(IError error)
    {
        _error = error;
    }



}