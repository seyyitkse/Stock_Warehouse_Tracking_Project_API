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
    public async Task<IActionResult> GetMovementTrend([FromQuery] string granularity = "daily", CancellationToken ct = default)
    {
        return Ok(await _reportService.GetMovementTrendAsync(granularity, ct));
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
        var contentType = format == "xlsx" ? "text/csv" : "text/csv";
        var fileName = format == "xlsx" ? "hareket-raporu.csv" : "hareket-raporu.csv";
        return File(bytes, contentType, fileName);
    }
}
