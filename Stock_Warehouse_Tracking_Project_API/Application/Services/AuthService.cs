using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Auth;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IOperationLogService _opLog;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IConfiguration config,
        IOperationLogService opLog,
        ILogger<AuthService> logger)
    {
        _db = db;
        _config = config;
        _opLog = opLog;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Login denemesi: {Email}", request.Email);

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Başarısız login: {Email}", request.Email);
            await _opLog.LogAsync(null, "Login", "AppUser", false, $"Email: {request.Email}", "Geçersiz e-posta veya şifre.",
                severity: Domain.Enums.EventLogSeverity.Warning, ct: ct);
            throw new UnauthorizedAccessException("Geçersiz e-posta veya şifre.");
        }

        var token = GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiresInMinutes"));

        _logger.LogInformation("Başarılı login: {Email}, Rol: {Role}", user.Email, user.Role.Name);
        await _opLog.LogAsync(user.UserId, "Login", "AppUser", true, $"Email: {user.Email}", ct: ct);

        return new LoginResponse
        {
            Token = token,
            UserName = user.Name,
            Role = user.Role.Name,
            ExpiresAt = expiresAt
        };
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Kayıt isteği: {Email}", request.Email);

        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (exists)
        {
            await _opLog.LogAsync(null, "Register", "AppUser", false, $"Email: {request.Email}", "E-posta zaten kayıtlı.",
                severity: Domain.Enums.EventLogSeverity.Warning, ct: ct);
            throw new InvalidOperationException("Bu e-posta adresi zaten kayıtlı.");
        }

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

        _logger.LogInformation("Yeni kullanıcı oluşturuldu: {Email}", user.Email);
        await _opLog.LogAsync(user.UserId, "Register", "AppUser", true, $"Email: {user.Email}", ct: ct);
    }

    private string GenerateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var expires = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiresInMinutes"));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
