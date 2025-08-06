using ForecastingGas.Dto.Responses;
using ForecastingGas.Dto.Requests;
using Microsoft.AspNetCore.Mvc;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Controllers;

[ApiController]
[Route("api/upload")]
public class File : ControllerBase
{
    private IDataProvider _file;

    public File(IDataProvider dataProvider)
    {
        _file = dataProvider;
    }


}