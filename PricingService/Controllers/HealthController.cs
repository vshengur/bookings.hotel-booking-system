using Microsoft.AspNetCore.Mvc;

namespace PricingService.Controllers;

/// <summary>
/// Health check controller for Consul and monitoring
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "pricing-service",
            timestamp = DateTime.UtcNow
        });
    }
}
