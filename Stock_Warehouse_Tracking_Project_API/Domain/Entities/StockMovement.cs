using Stock_Warehouse_Tracking_Project_API.Domain.Enums;

namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int MovementId { get; set; }
    public MovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; }
    public string? RefNo { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? SourceWarehouseId { get; set; }
    public Warehouse? SourceWarehouse { get; set; }

    public int? DestWarehouseId { get; set; }
    public Warehouse? DestWarehouse { get; set; }
}
