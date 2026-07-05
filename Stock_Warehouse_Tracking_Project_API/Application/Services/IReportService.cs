using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Report;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IReportService
{
    Task<StockSummaryReportDto> GetStockSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MovementTrendPointDto>> GetMovementTrendAsync(string granularity = "daily", CancellationToken ct = default);
    Task<IReadOnlyList<WarehouseComparisonDto>> GetWarehouseComparisonAsync(CancellationToken ct = default);
    Task<byte[]> ExportMovementsCsvAsync(CancellationToken ct = default);
}
