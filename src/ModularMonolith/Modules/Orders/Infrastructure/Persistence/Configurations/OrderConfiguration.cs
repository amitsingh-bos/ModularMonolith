using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Orders.Domain.Entities;

namespace ModularMonolith.Modules.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", "orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.ShippingAddressLine1).IsRequired().HasMaxLength(200);
        builder.Property(o => o.ShippingAddressLine2).HasMaxLength(200);
        builder.Property(o => o.ShippingCity).IsRequired().HasMaxLength(100);
        builder.Property(o => o.ShippingCountry).IsRequired().HasMaxLength(100);
        builder.Property(o => o.ShippingPostalCode).IsRequired().HasMaxLength(20);
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Status).HasConversion<int>();

        builder.HasIndex(o => new { o.OrderNumber, o.TenantId }).IsUnique();
        builder.HasIndex(o => new { o.TenantId, o.Status });

        builder.HasMany(o => o.Items)
               .WithOne()
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
