using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Alert;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IStockThresholdService
{
    Task<IReadOnlyList<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct = default);
    Task UpdateThresholdAsync(string matnr, string whId, decimal minLevel, CancellationToken ct = default);
    Task<int> GetLowStockCountAsync(CancellationToken ct = default);
}
