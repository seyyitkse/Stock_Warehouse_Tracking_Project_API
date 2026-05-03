using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.Common;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Logging;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class LogsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<LogsController> _logger;

    public LogsController(AppDbContext db, ILogger<LogsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OperationLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? entity,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] bool? isSuccess,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _db.OperationLogs
            .AsNoTracking()
            .Include(l => l.User)
            .AsQueryable();

        if (userId.HasValue)       query = query.Where(l => l.UserId == userId.Value);
        if (action is not null)    query = query.Where(l => l.Action.Contains(action));
        if (entity is not null)    query = query.Where(l => l.Entity.Contains(entity));
        if (dateFrom.HasValue)     query = query.Where(l => l.Timestamp >= dateFrom.Value);
        if (dateTo.HasValue)       query = query.Where(l => l.Timestamp <= dateTo.Value);
        if (isSuccess.HasValue)    query = query.Where(l => l.IsSuccess == isSuccess.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        _logger.LogInformation("Log sorgulama: TotalCount={Total}, Page={Page}", totalCount, page);

        return Ok(new PagedResult<OperationLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
}
