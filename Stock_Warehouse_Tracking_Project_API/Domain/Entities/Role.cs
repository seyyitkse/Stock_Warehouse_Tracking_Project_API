namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Permissions { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
