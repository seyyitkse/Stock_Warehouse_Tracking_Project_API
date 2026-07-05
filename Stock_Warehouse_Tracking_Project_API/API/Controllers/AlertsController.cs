using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Alert;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IStockThresholdService _thresholdService;

    public AlertsController(IStockThresholdService thresholdService)
    {
        _thresholdService = thresholdService;
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IReadOnlyList<LowStockAlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
    {
        var alerts = await _thresholdService.GetLowStockAlertsAsync(ct);
        return Ok(alerts);
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount(CancellationToken ct)
    {
        var count = await _thresholdService.GetLowStockCountAsync(ct);
        return Ok(new { count });
    }
}
