using System.Security.Claims;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    public int? UserId =>
        int.TryParse(_accessor.HttpContext?.User.FindFirstValue("userId"), out var id) ? id : null;

    public string? UserEmail =>
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public string? Role =>
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
}
