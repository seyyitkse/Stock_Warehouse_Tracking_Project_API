namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Alert;

public class LowStockAlertDto
{
    public string MaterialNo { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseId { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal MinLevel { get; set; }
    public decimal Deficit { get; set; }
}

public class UpdateStockThresholdRequest
{
    public decimal MinLevel { get; set; }
}

public class BulkStockInRequest
{
    public IReadOnlyList<Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock.StockInRequest> Items { get; set; } = [];
}

public class BulkStockInResultDto
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = [];
}
