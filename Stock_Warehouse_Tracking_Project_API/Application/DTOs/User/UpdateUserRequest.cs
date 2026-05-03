namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.User;

public class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
}
