using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Domain.Enums;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IIntegrationService
{
    Task<IReadOnlyList<IntegrationStatusDto>> GetIntegrationsAsync(CancellationToken ct = default);
    Task<IntegrationStatusDto?> GetIntegrationStatusAsync(string name, CancellationToken ct = default);
    Task<bool> SyncIntegrationAsync(string name, CancellationToken ct = default);
}

public class IntegrationStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "unknown";
    public string? Description { get; set; }
}

public class IntegrationService : IIntegrationService
{
    private readonly IConfiguration _configuration;
    private readonly IHealthStatusService _healthStatusService;
    private readonly IEnumerable<INotificationProvider> _notificationProviders;
    private readonly IStockThresholdService _thresholdService;
    private readonly IOperationLogService _opLog;
    private readonly INotificationProvider? _sendGrid;

    public IntegrationService(
        IConfiguration configuration,
        IHealthStatusService healthStatusService,
        IEnumerable<INotificationProvider> notificationProviders,
        IStockThresholdService thresholdService,
        IOperationLogService opLog)
    {
        _configuration = configuration;
        _healthStatusService = healthStatusService;
        _notificationProviders = notificationProviders;
        _thresholdService = thresholdService;
        _opLog = opLog;
        _sendGrid = notificationProviders.FirstOrDefault(p => p.Name == "SendGrid");
    }

    public async Task<IReadOnlyList<IntegrationStatusDto>> GetIntegrationsAsync(CancellationToken ct = default)
    {
        var health = await _healthStatusService.GetStatusAsync(ct);
        var provider = SapClientConfiguration.GetProvider(_configuration);

        var list = new List<IntegrationStatusDto>
        {
            new()
            {
                Name = "SAP",
                Status = health.Sap,
                Description = $"Provider: {provider}"
            }
        };

        foreach (var notify in _notificationProviders)
        {
            var available = await notify.IsAvailableAsync(ct);
            list.Add(new IntegrationStatusDto
            {
                Name = notify.Name,
                Status = available ? "configured" : "not_configured",
                Description = available ? "Bildirim provider hazır" : "API anahtarı eksik"
            });
        }

        return list;
    }

    public async Task<IntegrationStatusDto?> GetIntegrationStatusAsync(string name, CancellationToken ct = default)
    {
        var all = await GetIntegrationsAsync(ct);
        return all.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> SyncIntegrationAsync(string name, CancellationToken ct = default)
    {
        if (!name.Equals("SendGrid", StringComparison.OrdinalIgnoreCase) || _sendGrid is null)
            return false;

        var alerts = await _thresholdService.GetLowStockAlertsAsync(ct);
        if (alerts.Count == 0)
            return true;

        var to = _configuration["Integrations:SendGrid:AlertEmail"];
        if (string.IsNullOrWhiteSpace(to))
            return false;

        var body = string.Join("\n", alerts.Select(a =>
            $"- {a.ProductName} ({a.MaterialNo}) @ {a.WarehouseName}: {a.Quantity}/{a.MinLevel}"));
        var sent = await _sendGrid.SendEmailAsync(to, "Kritik Stok Uyarısı", body, ct: ct);
        await _opLog.LogAsync(
            null,
            "LowStockAlertEmail",
            "Notification",
            sent,
            details: $"To={to}, AlertCount={alerts.Count}",
            errorMessage: sent ? null : "SendGrid gönderimi başarısız.",
            source: EventLogSource.Integration,
            severity: sent ? EventLogSeverity.Info : EventLogSeverity.Error,
            ct: ct);
        return sent;
    }
}
