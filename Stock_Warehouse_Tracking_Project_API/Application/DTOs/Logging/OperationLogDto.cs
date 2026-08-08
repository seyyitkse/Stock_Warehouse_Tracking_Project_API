namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Logging;

public class OperationLogDto
{
    public long LogId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorUserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string Source { get; set; } = "User";
    public string Severity { get; set; } = "Info";
}

public class LogMetaDto
{
    public IReadOnlyList<string> Actions { get; set; } = [];
    public IReadOnlyList<string> Entities { get; set; } = [];
    public IReadOnlyList<string> Sources { get; set; } = [];
    public IReadOnlyList<string> Severities { get; set; } = [];
}
