namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;

public class UpdateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
}
