namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserEmail { get; }
    string? Role { get; }
}
