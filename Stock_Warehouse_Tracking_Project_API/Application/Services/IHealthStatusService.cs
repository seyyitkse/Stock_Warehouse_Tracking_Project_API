using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Health;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IHealthStatusService
{
    Task<HealthStatusDto> GetStatusAsync(CancellationToken ct = default);
}
