namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;

public class StockOutRequest
{
    public string MaterialNo { get; set; } = string.Empty;
    public string WarehouseId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? RefNo { get; set; }
}
