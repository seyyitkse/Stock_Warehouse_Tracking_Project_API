using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.RoleId);
        builder.Property(r => r.Name).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Permissions).HasMaxLength(500);

        builder.HasData(
            new Role { RoleId = 1, Name = "Admin" },
            new Role { RoleId = 2, Name = "WarehouseManager" },
            new Role { RoleId = 3, Name = "Manager" },
            new Role { RoleId = 4, Name = "SuperAdmin" }
        );
    }
}
