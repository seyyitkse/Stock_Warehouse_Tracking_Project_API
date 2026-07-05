using Stock_Warehouse_Tracking_Project_API.Application.Common;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Logging;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface ILogService
{
    Task<PagedResult<OperationLogDto>> GetLogsAsync(LogFilterRequest filter, CancellationToken ct = default);
}

public class LogFilterRequest
{
    public int? UserId { get; set; }
    public string? Action { get; set; }
    public string? Entity { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsSuccess { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
