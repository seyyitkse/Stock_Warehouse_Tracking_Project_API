namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap.Http;

internal sealed class SapStockJsonDto
{
    public string? Matnr { get; set; }
    public string? WhId { get; set; }
    public decimal Quantity { get; set; }
    public string? UpdatedAt { get; set; }
}

internal sealed class SapMovementJsonResponse
{
    public bool Success { get; set; }
    public string? SapDocNo { get; set; }
    public string? ErrorMessage { get; set; }
}

internal sealed class SapCreateProductJsonRequest
{
    public string Matnr { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
}

internal sealed class SapStockMovementJsonRequest
{
    public string Matnr { get; set; } = string.Empty;
    public string WhId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? RefNo { get; set; }
}

internal sealed class SapTransferJsonRequest
{
    public string Matnr { get; set; } = string.Empty;
    public string SourceWhId { get; set; } = string.Empty;
    public string DestWhId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? RefNo { get; set; }
}
