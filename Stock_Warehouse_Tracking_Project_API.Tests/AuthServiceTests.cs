using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Auth;
using Stock_Warehouse_Tracking_Project_API.Application.Services;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

namespace Stock_Warehouse_Tracking_Project_API.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        await using var db = TestDbFactory.Create(nameof(LoginAsync_WithValidCredentials_ReturnsToken));
        db.Roles.Add(new Role { RoleId = 4, Name = "SuperAdmin" });
        db.Users.Add(new AppUser
        {
            UserId = 1,
            Name = "Test Admin",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            RoleId = 4
        });
        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS_!@#$%",
                ["Jwt:Issuer"] = "StockWarehouseAPI",
                ["Jwt:Audience"] = "StockWarehouseClient",
                ["Jwt:ExpiresInMinutes"] = "480"
            })
            .Build();

        var opLog = new Mock<IOperationLogService>();
        var service = new AuthService(db, config, opLog.Object, NullLogger<AuthService>.Instance);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "Admin123!"
        });

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("SuperAdmin", result.Role);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorized()
    {
        await using var db = TestDbFactory.Create(nameof(LoginAsync_WithInvalidPassword_ThrowsUnauthorized));
        db.Roles.Add(new Role { RoleId = 4, Name = "SuperAdmin" });
        db.Users.Add(new AppUser
        {
            UserId = 1,
            Name = "Test Admin",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            RoleId = 4
        });
        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS_!@#$%",
                ["Jwt:Issuer"] = "StockWarehouseAPI",
                ["Jwt:Audience"] = "StockWarehouseClient",
                ["Jwt:ExpiresInMinutes"] = "480"
            })
            .Build();

        var opLog = new Mock<IOperationLogService>();
        var service = new AuthService(db, config, opLog.Object, NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest { Email = "test@example.com", Password = "wrong" }));
    }
}
