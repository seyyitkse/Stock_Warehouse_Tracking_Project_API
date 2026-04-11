using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Auth;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}
