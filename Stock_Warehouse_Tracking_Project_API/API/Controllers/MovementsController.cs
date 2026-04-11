using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Application.Common;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Movement;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.API.Controllers;

[ApiController]
[Route("api/movements")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly IMovementService _movementService;

    public MovementsController(IMovementService movementService)
    {
        _movementService = movementService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MovementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovements([FromQuery] MovementFilterRequest filter, CancellationToken ct)
    {
        var result = await _movementService.GetMovementsAsync(filter, ct);
        return Ok(result);
    }
}
