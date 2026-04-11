using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehousesController> _logger;

    public WarehousesController(IWarehouseService warehouseService, ILogger<WarehousesController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var warehouses = await _warehouseService.GetAllAsync(ct);
        return Ok(warehouses);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id, ct);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request, CancellationToken ct)
    {
        var warehouse = await _warehouseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.WarehouseId }, warehouse);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct)
    {
        var warehouse = await _warehouseService.UpdateAsync(id, request, ct);
        return Ok(warehouse);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _warehouseService.DeleteAsync(id, ct);
        return NoContent();
    }
}
