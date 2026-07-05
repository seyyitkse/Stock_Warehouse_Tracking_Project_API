using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Health;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthStatusController : ControllerBase
{
    private readonly IHealthStatusService _healthStatusService;

    public HealthStatusController(IHealthStatusService healthStatusService)
    {
        _healthStatusService = healthStatusService;
    }

    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var status = await _healthStatusService.GetStatusAsync(ct);
        return Ok(status);
    }
}
