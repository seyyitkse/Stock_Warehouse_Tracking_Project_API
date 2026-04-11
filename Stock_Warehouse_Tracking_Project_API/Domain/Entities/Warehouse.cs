namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class Warehouse : BaseEntity
{
    public int WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }

    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}
