using ForecastingGas.Data.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers.CRUD;

[ApiController]
[Route("api/Delete")]
public class DeleteData : ControllerBase
{
    private IDeleteData _deleteData;

    public DeleteData(IDeleteData deleteData)
    {
        _deleteData = deleteData;
    }

    [HttpDelete("AllActualData")]
    public async Task<IActionResult> DeleteAllData()
    {
        try
        {
            var result = await _deleteData.DeleteAllData();

            return Ok(new
            {
                response = $"Successfully deleted all data. {result}"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("ActualDataByColumnName")]
    public async Task<IActionResult> DeleteDataById(int id)
    {
        try
        {
            var result = await _deleteData.DeleteDataById(id);

            return Ok(new
            {
                response = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}