using Microsoft.AspNetCore.Mvc;

namespace Vessel.API.Controllers;

/// <summary>
/// Provides health check endpoints to monitor the API status.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Checks the health status of the Vessel API.
    /// </summary>
    /// <returns>A status message indicating that the API is running.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult Get() => Ok(new { status = "Healthy! Vessel API is running" });
}