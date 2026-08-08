using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Report;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("stock-summary")]
    [ProducesResponseType(typeof(StockSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockSummary(CancellationToken ct)
    {
        return Ok(await _reportService.GetStockSummaryAsync(ct));
    }

    [HttpGet("movement-trend")]
    [ProducesResponseType(typeof(IReadOnlyList<MovementTrendPointDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovementTrend(
        [FromQuery] string granularity = "daily",
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        CancellationToken ct = default)
    {
        return Ok(await _reportService.GetMovementTrendAsync(granularity, dateFrom, dateTo, ct));
    }

    [HttpGet("warehouse-comparison")]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseComparisonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarehouseComparison(CancellationToken ct)
    {
        return Ok(await _reportService.GetWarehouseComparisonAsync(ct));
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export([FromQuery] string format = "csv", CancellationToken ct = default)
    {
        var bytes = await _reportService.ExportMovementsCsvAsync(ct);
        var contentType = "text/csv";
        var fileName = "hareket-raporu.csv";
        return File(bytes, contentType, fileName);
    }

    [HttpPost("email")]
    [ProducesResponseType(typeof(EmailReportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EmailReport([FromBody] EmailReportRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue("userId");
        int? userId = int.TryParse(userIdClaim, out var id) ? id : null;
        var result = await _reportService.EmailReportAsync(request, userId, ct);
        return result.Sent ? Ok(result) : BadRequest(result);
    }
}
