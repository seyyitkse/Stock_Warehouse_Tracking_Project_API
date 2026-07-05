using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Health;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class HealthStatusService : IHealthStatusService
{
    private readonly AppDbContext _db;
    private readonly HealthCheckService _healthCheckService;

    public HealthStatusService(AppDbContext db, HealthCheckService healthCheckService)
    {
        _db = db;
        _healthCheckService = healthCheckService;
    }

    public async Task<HealthStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        var dbStatus = "healthy";
        try
        {
            await _db.Database.CanConnectAsync(ct);
        }
        catch
        {
            dbStatus = "unhealthy";
        }

        var sapStatus = "unknown";
        try
        {
            var report = await _healthCheckService.CheckHealthAsync(
                registration => registration.Name == "sap",
                ct);
            sapStatus = report.Status == HealthStatus.Healthy ? "healthy" : "unhealthy";
        }
        catch
        {
            sapStatus = "unhealthy";
        }

        return new HealthStatusDto
        {
            Api = "healthy",
            Database = dbStatus,
            Sap = sapStatus
        };
    }
}
