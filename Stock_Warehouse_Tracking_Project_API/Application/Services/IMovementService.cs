using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Movement;
using Stock_Warehouse_Tracking_Project_API.Application.Common;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IMovementService
{
    Task<PagedResult<MovementDto>> GetMovementsAsync(MovementFilterRequest filter, CancellationToken ct = default);
}
