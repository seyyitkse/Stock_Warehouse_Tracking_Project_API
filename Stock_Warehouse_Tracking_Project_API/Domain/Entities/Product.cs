namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class Product : BaseEntity
{
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Barcode { get; set; }
    public decimal MinStock { get; set; }

    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}
