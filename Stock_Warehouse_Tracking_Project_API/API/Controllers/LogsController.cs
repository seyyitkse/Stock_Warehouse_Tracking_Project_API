using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.Common;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Logging;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
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
        [FromQuery] string? source,
        [FromQuery] string? severity,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _logService.GetLogsAsync(new LogFilterRequest
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            DateFrom = dateFrom,
            DateTo = dateTo,
            IsSuccess = isSuccess,
            Source = source,
            Severity = severity,
            Q = q,
            Page = page,
            PageSize = pageSize
        }, ct);

        return Ok(result);
    }

    [HttpGet("meta")]
    [ProducesResponseType(typeof(LogMetaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMeta(CancellationToken ct)
    {
        return Ok(await _logService.GetMetaAsync(ct));
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export(
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? entity,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] bool? isSuccess,
        [FromQuery] string? source,
        [FromQuery] string? severity,
        [FromQuery] string? q,
        CancellationToken ct = default)
    {
        var bytes = await _logService.ExportCsvAsync(new LogFilterRequest
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            DateFrom = dateFrom,
            DateTo = dateTo,
            IsSuccess = isSuccess,
            Source = source,
            Severity = severity,
            Q = q,
            Page = 1,
            PageSize = 5000
        }, ct);
        return File(bytes, "text/csv", "event-logs.csv");
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(OperationLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var log = await _logService.GetByIdAsync(id, ct);
        return log is null ? NotFound() : Ok(log);
    }
}
