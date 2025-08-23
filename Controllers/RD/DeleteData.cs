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

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("ActualDataByColumnName")]
    public async Task<IActionResult> DeleteDataByColumnName(string columnName)
    {
        try
        {
            var result = await _deleteData.DeleteDataByColumnName(columnName);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}