using Microsoft.AspNetCore.Mvc;

namespace ForecastingGas.Controllers;

/// <summary>
/// Lightweight ping endpoint used by the GitHub Actions keepalive workflow
/// to prevent Render free-tier cold starts on weekdays 6am–5pm PHT.
/// </summary>
[ApiController]
[Route("api")]
public class KeepAliveController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "alive", utc = DateTime.UtcNow });
}
