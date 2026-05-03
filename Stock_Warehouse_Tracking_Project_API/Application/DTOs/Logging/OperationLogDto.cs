namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Logging;

public class OperationLogDto
{
    public long LogId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
