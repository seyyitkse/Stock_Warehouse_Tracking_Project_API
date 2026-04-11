using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class OperationLogService : IOperationLogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OperationLogService> _logger;

    public OperationLogService(AppDbContext db, ILogger<OperationLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(
        int? userId,
        string action,
        string entity,
        bool isSuccess,
        string? details = null,
        string? errorMessage = null,
        CancellationToken ct = default)
    {
        try
        {
            _db.OperationLogs.Add(new OperationLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                IsSuccess = isSuccess,
                Details = details,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Log kaydı asıl hatayı gizlememeli
            _logger.LogError(ex, "OperationLog kaydedilemedi. Action={Action}, Entity={Entity}", action, entity);
        }
    }
}
