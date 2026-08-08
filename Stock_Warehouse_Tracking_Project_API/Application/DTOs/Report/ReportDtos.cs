namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Report;

public class StockSummaryReportDto
{
    public decimal TotalQuantity { get; set; }
    public int ProductCount { get; set; }
    public int WarehouseCount { get; set; }
    public int LowStockCount { get; set; }
    public int EmptyStockLines { get; set; }
}

public class MovementTrendPointDto
{
    public string Label { get; set; } = string.Empty;
    public int InCount { get; set; }
    public int OutCount { get; set; }
    public int TransferCount { get; set; }
}

public class WarehouseComparisonDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public int LineCount { get; set; }
}

public class EmailReportRequest
{
    public string? To { get; set; }
    public int PeriodDays { get; set; } = 7;
    public bool IncludeCsv { get; set; } = true;
}

public class EmailReportResultDto
{
    public bool Sent { get; set; }
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
