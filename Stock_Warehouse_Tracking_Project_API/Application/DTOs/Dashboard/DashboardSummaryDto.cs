using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Movement;

namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public string SapStatus { get; set; } = "unknown";
    public int ProductCount { get; set; }
    public int WarehouseCount { get; set; }
    public decimal TotalStockQuantity { get; set; }
    public int EmptyStockLines { get; set; }
    public int SapOnlyProductCount { get; set; }
    public int LowStockCount { get; set; }
    public IReadOnlyList<MovementDto> RecentMovements { get; set; } = [];
    public IReadOnlyList<DistributionItemDto> WarehouseStockDistribution { get; set; } = [];
    public IReadOnlyList<DistributionItemDto> CategoryStockDistribution { get; set; } = [];
}

public class DistributionItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
