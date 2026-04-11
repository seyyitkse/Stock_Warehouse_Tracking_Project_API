namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class AppUser : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
    public ICollection<OperationLog> Logs { get; set; } = new List<OperationLog>();
}
