using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.User;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _db;
    private readonly IOperationLogService _opLog;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        AppDbContext db,
        IOperationLogService opLog,
        ICurrentUserService currentUser,
        ILogger<UserManagementService> logger)
    {
        _db = db;
        _opLog = opLog;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => MapToDto(u))
            .ToListAsync(ct);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id, ct);

        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.Email, ct);

        if (exists)
            throw new InvalidOperationException("Bu e-posta adresi zaten kayıtlı.");

        var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == request.RoleId, ct);
        if (!roleExists)
            throw new KeyNotFoundException($"RoleId={request.RoleId} bulunamadı.");

        var user = new AppUser
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(user).Reference(u => u.Role).LoadAsync(ct);

        _logger.LogInformation("Yeni kullanıcı oluşturuldu: {Email}, Rol: {RoleId}", user.Email, user.RoleId);
        await _opLog.LogAsync(user.UserId, "CreateUser", "AppUser", true,
            $"Email: {user.Email}, RoleId: {user.RoleId}",
            actorUserId: _currentUser.UserId, ct: ct);

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id, ct)
            ?? throw new KeyNotFoundException($"Kullanıcı bulunamadı. (Id={id})");

        var emailTaken = await _db.Users
            .AnyAsync(u => u.Email == request.Email && u.UserId != id, ct);

        if (emailTaken)
            throw new InvalidOperationException("Bu e-posta adresi başka bir kullanıcıya ait.");

        var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == request.RoleId, ct);
        if (!roleExists)
            throw new KeyNotFoundException($"RoleId={request.RoleId} bulunamadı.");

        user.Name = request.Name;
        user.Email = request.Email;
        user.RoleId = request.RoleId;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _db.Entry(user).Reference(u => u.Role).LoadAsync(ct);

        _logger.LogInformation("Kullanıcı güncellendi: {UserId}", id);
        await _opLog.LogAsync(id, "UpdateUser", "AppUser", true,
            $"Email: {user.Email}, RoleId: {user.RoleId}",
            actorUserId: _currentUser.UserId, ct: ct);

        return MapToDto(user);
    }

    public async Task ChangeRoleAsync(int id, ChangeRoleRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == id, ct)
            ?? throw new KeyNotFoundException($"Kullanıcı bulunamadı. (Id={id})");

        var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == request.RoleId, ct);
        if (!roleExists)
            throw new KeyNotFoundException($"RoleId={request.RoleId} bulunamadı.");

        var oldRoleId = user.RoleId;
        user.RoleId = request.RoleId;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Kullanıcı rolü değiştirildi: {UserId}, {OldRole} → {NewRole}", id, oldRoleId, request.RoleId);
        await _opLog.LogAsync(id, "ChangeRole", "AppUser", true,
            $"OldRoleId: {oldRoleId}, NewRoleId: {request.RoleId}",
            actorUserId: _currentUser.UserId, ct: ct);
    }

    public async Task DeleteUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == id, ct)
            ?? throw new KeyNotFoundException($"Kullanıcı bulunamadı. (Id={id})");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Kullanıcı silindi (soft): {UserId}", id);
        await _opLog.LogAsync(id, "DeleteUser", "AppUser", true,
            $"Email: {user.Email}",
            actorUserId: _currentUser.UserId, ct: ct);
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default)
    {
        return await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.RoleId)
            .Select(r => new RoleDto { RoleId = r.RoleId, Name = r.Name })
            .ToListAsync(ct);
    }

    private static UserDto MapToDto(AppUser user) => new()
    {
        UserId = user.UserId,
        Name = user.Name,
        Email = user.Email,
        RoleId = user.RoleId,
        RoleName = user.Role?.Name ?? string.Empty,
        CreatedAt = user.CreatedAt,
        IsDeleted = user.IsDeleted
    };
}
