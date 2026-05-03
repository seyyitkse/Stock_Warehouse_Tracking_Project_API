using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/stocks")]
[Authorize]
public class StocksController : ControllerBase
{
    private readonly INewStockService _stockService;
    private readonly ILogger<StocksController> _logger;

    public StocksController(INewStockService stockService, ILogger<StocksController> logger)
    {
        _stockService = stockService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StockDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStocks([FromQuery] string? matnr, [FromQuery] string? whId, CancellationToken ct)
    {
        var stocks = await _stockService.GetStocksAsync(matnr, whId, ct);
        return Ok(stocks);
    }

    [HttpGet("{matnr}/{whId}")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(string matnr, string whId, CancellationToken ct)
    {
        var stock = await _stockService.GetStockDetailAsync(matnr, whId, ct);
        return stock is null ? NotFound() : Ok(stock);
    }

    [HttpPost("in")]
    [Authorize(Roles = "SuperAdmin,Admin,WarehouseManager")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StockIn([FromBody] StockInRequest request, CancellationToken ct)
    {
        var result = await _stockService.StockInAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("out")]
    [Authorize(Roles = "SuperAdmin,Admin,WarehouseManager")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StockOut([FromBody] StockOutRequest request, CancellationToken ct)
    {
        var result = await _stockService.StockOutAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("transfer")]
    [Authorize(Roles = "SuperAdmin,Admin,WarehouseManager")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer([FromBody] StockTransferRequest request, CancellationToken ct)
    {
        var result = await _stockService.TransferAsync(request, ct);
        return Ok(result);
    }
}
