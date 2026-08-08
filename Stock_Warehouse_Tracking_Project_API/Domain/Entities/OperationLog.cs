namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class OperationLog
{
    public long LogId { get; set; }
    public int? UserId { get; set; }
    public AppUser? User { get; set; }
    public int? ActorUserId { get; set; }
    public AppUser? ActorUser { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string Source { get; set; } = "User";
    public string Severity { get; set; } = "Info";
}
