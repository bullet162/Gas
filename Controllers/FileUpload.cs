using ForecastingGas.Data.DataProviders;
using ForecastingGas.Dto.Requests;
using Microsoft.AspNetCore.Mvc;

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