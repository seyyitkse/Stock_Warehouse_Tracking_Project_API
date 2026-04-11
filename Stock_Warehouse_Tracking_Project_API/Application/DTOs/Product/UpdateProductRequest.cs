namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Product;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Barcode { get; set; }
}
