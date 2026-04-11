namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;

public class StockTransferRequest
{
    public string MaterialNo { get; set; } = string.Empty;
    public string SourceWarehouseId { get; set; } = string.Empty;
    public string DestWarehouseId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? RefNo { get; set; }
}
