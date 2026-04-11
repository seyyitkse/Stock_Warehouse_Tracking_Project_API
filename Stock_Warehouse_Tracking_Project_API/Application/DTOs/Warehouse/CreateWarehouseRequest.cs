namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;

public class CreateWarehouseRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
}
