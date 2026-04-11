using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(w => w.WarehouseId);
        builder.Property(w => w.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(w => w.Code).IsUnique();
        builder.Property(w => w.Name).HasMaxLength(150).IsRequired();
        builder.Property(w => w.Location).HasMaxLength(250);

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
