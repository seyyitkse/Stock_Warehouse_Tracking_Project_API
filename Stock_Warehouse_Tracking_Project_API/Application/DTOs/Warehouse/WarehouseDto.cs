namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;

public class WarehouseDto
{
    public int WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
}
