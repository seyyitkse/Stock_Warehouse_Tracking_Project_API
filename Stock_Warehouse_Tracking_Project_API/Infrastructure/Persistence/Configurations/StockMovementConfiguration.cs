using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(m => m.MovementId);
        builder.Property(m => m.Quantity).HasPrecision(18, 4);
        builder.Property(m => m.RefNo).HasMaxLength(100);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Movements)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Product)
            .WithMany(p => p.Movements)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.SourceWarehouse)
            .WithMany()
            .HasForeignKey(m => m.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.DestWarehouse)
            .WithMany()
            .HasForeignKey(m => m.DestWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
