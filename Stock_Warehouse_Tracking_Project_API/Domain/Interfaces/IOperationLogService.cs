namespace Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

public interface IOperationLogService
{
    Task LogAsync(
        int? userId,
        string action,
        string entity,
        bool isSuccess,
        string? details = null,
        string? errorMessage = null,
        string? source = null,
        string? severity = null,
        int? actorUserId = null,
        CancellationToken ct = default);
}
