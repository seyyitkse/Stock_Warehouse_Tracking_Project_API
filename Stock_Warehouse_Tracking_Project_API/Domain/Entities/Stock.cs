namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class Stock : BaseEntity
{
    public int StockId { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal MinLevel { get; set; }
}
