using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence.Configurations;

public class OperationLogConfiguration : IEntityTypeConfiguration<OperationLog>
{
    public void Configure(EntityTypeBuilder<OperationLog> builder)
    {
        builder.HasKey(l => l.LogId);
        builder.Property(l => l.Action).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Entity).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Details).HasColumnType("nvarchar(max)");
        builder.Property(l => l.ErrorMessage).HasMaxLength(2000);
        builder.Property(l => l.Timestamp).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(l => l.User)
            .WithMany(u => u.Logs)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
