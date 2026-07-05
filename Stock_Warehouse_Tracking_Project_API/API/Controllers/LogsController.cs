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
            Page = page,
            PageSize = pageSize
        }, ct);

        return Ok(result);
    }
}
