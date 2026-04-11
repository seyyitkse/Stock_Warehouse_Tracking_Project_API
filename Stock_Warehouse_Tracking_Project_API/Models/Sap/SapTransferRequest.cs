namespace Stock_Warehouse_Tracking_Project_API.Models.Sap;

public class SapTransferRequest
{
    public string Matnr { get; set; } = string.Empty;
    public string SourceWhId { get; set; } = string.Empty;
    public string DestWhId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? RefNo { get; set; }
}
