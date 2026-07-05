using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Dashboard;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}
