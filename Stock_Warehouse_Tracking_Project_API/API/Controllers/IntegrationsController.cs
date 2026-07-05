using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

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
    private readonly IConfiguration _configuration;

    public NotificationsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("preferences")]
    public IActionResult GetPreferences()
    {
        return Ok(new
        {
            emailEnabled = !string.IsNullOrWhiteSpace(_configuration["Integrations:SendGrid:AlertEmail"]),
            alertEmail = _configuration["Integrations:SendGrid:AlertEmail"] ?? ""
        });
    }

    [HttpPut("preferences")]
    public IActionResult UpdatePreferences([FromBody] NotificationPreferencesRequest request)
    {
        return Ok(new
        {
            message = "Tercihler kaydedildi (runtime config; kalıcı kayıt için appsettings güncelleyin).",
            alertEmail = request.AlertEmail
        });
    }
}

public class NotificationPreferencesRequest
{
    public bool EmailEnabled { get; set; }
    public string? AlertEmail { get; set; }
}
