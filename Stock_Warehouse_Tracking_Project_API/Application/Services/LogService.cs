using System.Text;
using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.Common;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Logging;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface ILogService
{
    Task<PagedResult<OperationLogDto>> GetLogsAsync(LogFilterRequest filter, CancellationToken ct = default);
    Task<OperationLogDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<LogMetaDto> GetMetaAsync(CancellationToken ct = default);
    Task<byte[]> ExportCsvAsync(LogFilterRequest filter, CancellationToken ct = default);
}

public class LogFilterRequest
{
    public int? UserId { get; set; }
    public string? Action { get; set; }
    public string? Entity { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsSuccess { get; set; }
    public string? Source { get; set; }
    public string? Severity { get; set; }
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

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
        var query = BuildQuery(filter);
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(MapProjection)
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

    public async Task<OperationLogDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.OperationLogs
            .AsNoTracking()
            .Where(l => l.LogId == id)
            .Select(MapProjection)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<LogMetaDto> GetMetaAsync(CancellationToken ct = default)
    {
        var actions = await _db.OperationLogs.AsNoTracking()
            .Select(l => l.Action).Distinct().OrderBy(x => x).Take(200).ToListAsync(ct);
        var entities = await _db.OperationLogs.AsNoTracking()
            .Select(l => l.Entity).Distinct().OrderBy(x => x).Take(200).ToListAsync(ct);
        var sources = await _db.OperationLogs.AsNoTracking()
            .Select(l => l.Source).Distinct().OrderBy(x => x).ToListAsync(ct);
        var severities = await _db.OperationLogs.AsNoTracking()
            .Select(l => l.Severity).Distinct().OrderBy(x => x).ToListAsync(ct);

        return new LogMetaDto
        {
            Actions = actions,
            Entities = entities,
            Sources = sources.Count > 0 ? sources : ["User", "System", "Integration"],
            Severities = severities.Count > 0 ? severities : ["Info", "Warning", "Error"]
        };
    }

    public async Task<byte[]> ExportCsvAsync(LogFilterRequest filter, CancellationToken ct = default)
    {
        var rows = await BuildQuery(filter)
            .OrderByDescending(l => l.Timestamp)
            .Take(5000)
            .Select(MapProjection)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("LogId,Timestamp,Source,Severity,Action,Entity,Actor,User,IsSuccess,Details,Error");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                r.LogId,
                r.Timestamp.ToString("O"),
                Escape(r.Source),
                Escape(r.Severity),
                Escape(r.Action),
                Escape(r.Entity),
                Escape(r.ActorUserName ?? r.ActorUserId?.ToString()),
                Escape(r.UserName ?? r.UserId.ToString()),
                r.IsSuccess,
                Escape(r.Details),
                Escape(r.ErrorMessage)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private IQueryable<Domain.Entities.OperationLog> BuildQuery(LogFilterRequest filter)
    {
        var query = _db.OperationLogs.AsNoTracking().AsQueryable();

        if (filter.UserId.HasValue) query = query.Where(l => l.UserId == filter.UserId.Value || l.ActorUserId == filter.UserId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Action)) query = query.Where(l => l.Action.Contains(filter.Action));
        if (!string.IsNullOrWhiteSpace(filter.Entity)) query = query.Where(l => l.Entity.Contains(filter.Entity));
        if (filter.DateFrom.HasValue) query = query.Where(l => l.Timestamp >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(l => l.Timestamp <= filter.DateTo.Value);
        if (filter.IsSuccess.HasValue) query = query.Where(l => l.IsSuccess == filter.IsSuccess.Value);
        if (!string.IsNullOrWhiteSpace(filter.Source)) query = query.Where(l => l.Source == filter.Source);
        if (!string.IsNullOrWhiteSpace(filter.Severity)) query = query.Where(l => l.Severity == filter.Severity);
        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var q = filter.Q;
            query = query.Where(l =>
                (l.Details != null && l.Details.Contains(q)) ||
                (l.ErrorMessage != null && l.ErrorMessage.Contains(q)) ||
                l.Action.Contains(q) ||
                l.Entity.Contains(q));
        }

        return query;
    }

    private static System.Linq.Expressions.Expression<Func<Domain.Entities.OperationLog, OperationLogDto>> MapProjection =>
        l => new OperationLogDto
        {
            LogId = l.LogId,
            UserId = l.UserId ?? 0,
            UserName = l.User != null ? l.User.Name : null,
            ActorUserId = l.ActorUserId,
            ActorUserName = l.ActorUser != null ? l.ActorUser.Name : null,
            Action = l.Action,
            Entity = l.Entity,
            Details = l.Details,
            Timestamp = l.Timestamp,
            IsSuccess = l.IsSuccess,
            ErrorMessage = l.ErrorMessage,
            Source = l.Source,
            Severity = l.Severity
        };

    private static string Escape(string? value)
    {
        var text = value ?? "";
        return text.Contains(',') || text.Contains('"') || text.Contains('\n')
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }
}
