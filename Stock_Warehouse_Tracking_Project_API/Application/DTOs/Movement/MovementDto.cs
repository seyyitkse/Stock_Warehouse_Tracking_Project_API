using Stock_Warehouse_Tracking_Project_API.Domain.Enums;

namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Movement;

public class MovementDto
{
    public int MovementId { get; set; }
    public MovementType Type { get; set; }
    public string TypeName => Type.ToString();
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; }
    public string? RefNo { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string? SourceWarehouseCode { get; set; }
    public string? DestWarehouseCode { get; set; }
    public string UserName { get; set; } = string.Empty;
}
