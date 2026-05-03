using Stock_Warehouse_Tracking_Project_API.Application.DTOs.User;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task ChangeRoleAsync(int id, ChangeRoleRequest request, CancellationToken ct = default);
    Task DeleteUserAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default);
}
