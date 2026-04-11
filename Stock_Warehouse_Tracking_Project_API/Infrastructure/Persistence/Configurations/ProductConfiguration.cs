using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Unit).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Category).HasMaxLength(100);
        builder.Property(p => p.Barcode).HasMaxLength(100);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
