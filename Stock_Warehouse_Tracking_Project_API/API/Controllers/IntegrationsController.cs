using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.Services;
using System.Security.Claims;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/integrations")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class IntegrationsController : ControllerBase
{
    private readonly IIntegrationService _integrationService;

    public IntegrationsController(IIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await _integrationService.GetIntegrationsAsync(ct));
    }

    [HttpGet("{name}/status")]
    public async Task<IActionResult> GetStatus(string name, CancellationToken ct)
    {
        var status = await _integrationService.GetIntegrationStatusAsync(name, ct);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpPost("{name}/sync")]
    public async Task<IActionResult> Sync(string name, CancellationToken ct)
    {
        var ok = await _integrationService.SyncIntegrationAsync(name, ct);
        return ok ? Ok(new { message = "Senkronizasyon tamamlandı." }) : BadRequest(new { message = "Senkronizasyon başarısız." });
    }
}

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationPreferenceService _preferenceService;

    public NotificationsController(INotificationPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await _preferenceService.GetForUserAsync(userId.Value, ct));
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var updated = await _preferenceService.UpsertForUserAsync(userId.Value, request, ct);
        return Ok(updated);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue("userId");
        return int.TryParse(claim, out var id) ? id : null;
    }
}
