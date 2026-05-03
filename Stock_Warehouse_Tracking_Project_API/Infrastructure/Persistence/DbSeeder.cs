using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedSuperAdminAsync(AppDbContext db)
    {
        const string superAdminEmail = "ahmet@superadmin.com";
        const int superAdminRoleId = 4;

        var exists = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == superAdminEmail);

        if (exists) return;

        var roleExists = await db.Roles.AnyAsync(r => r.RoleId == superAdminRoleId);
        if (!roleExists) return;

        var user = new AppUser
        {
            Name = "Ahmet Seyyit Köse",
            Email = superAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            RoleId = superAdminRoleId,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
