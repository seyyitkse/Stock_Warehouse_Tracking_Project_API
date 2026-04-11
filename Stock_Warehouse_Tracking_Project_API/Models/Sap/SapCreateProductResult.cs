namespace Stock_Warehouse_Tracking_Project_API.Models.Sap;

public class SapCreateProductResult
{
    public bool Success { get; set; }
    public string? SapDocNo { get; set; }
    public string? ErrorMessage { get; set; }
}
