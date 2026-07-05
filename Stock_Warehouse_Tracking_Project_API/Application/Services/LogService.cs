using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.Common;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Logging;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class LogService : ILogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<LogService> _logger;

    public LogService(AppDbContext db, ILogger<LogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<OperationLogDto>> GetLogsAsync(LogFilterRequest filter, CancellationToken ct = default)
    {
        var query = _db.OperationLogs
            .AsNoTracking()
            .Include(l => l.User)
            .AsQueryable();

        if (filter.UserId.HasValue) query = query.Where(l => l.UserId == filter.UserId.Value);
        if (filter.Action is not null) query = query.Where(l => l.Action.Contains(filter.Action));
        if (filter.Entity is not null) query = query.Where(l => l.Entity.Contains(filter.Entity));
        if (filter.DateFrom.HasValue) query = query.Where(l => l.Timestamp >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(l => l.Timestamp <= filter.DateTo.Value);
        if (filter.IsSuccess.HasValue) query = query.Where(l => l.IsSuccess == filter.IsSuccess.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(l => new OperationLogDto
            {
                LogId = l.LogId,
                UserId = l.UserId ?? 0,
                UserName = l.User != null ? l.User.Name : null,
                Action = l.Action,
                Entity = l.Entity,
                Details = l.Details,
                Timestamp = l.Timestamp,
                IsSuccess = l.IsSuccess,
                ErrorMessage = l.ErrorMessage
            })
            .ToListAsync(ct);

        _logger.LogInformation("Log sorgulama: TotalCount={Total}, Page={Page}", totalCount, filter.Page);

        return new PagedResult<OperationLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
