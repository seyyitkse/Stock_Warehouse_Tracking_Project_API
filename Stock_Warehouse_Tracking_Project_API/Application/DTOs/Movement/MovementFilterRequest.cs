using Stock_Warehouse_Tracking_Project_API.Domain.Enums;

namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Movement;

public class MovementFilterRequest
{
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public MovementType? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
