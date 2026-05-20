namespace Stock_Warehouse_Tracking_Project_API.Models.Sap;

public sealed class SapProductRow
{
    public string Matnr { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Barcode { get; set; }
    public DateTime CreatedAt { get; set; }
}
