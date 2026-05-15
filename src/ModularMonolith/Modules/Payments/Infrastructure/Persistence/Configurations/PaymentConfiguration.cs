using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Payments.Domain.Entities;
using ModularMonolith.Modules.Payments.Domain.Enums;

namespace ModularMonolith.Modules.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Version)
            .HasColumnName("version")
            .HasColumnType("integer")
            .HasDefaultValue(1)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
        builder.Property(p => p.TransactionReference).HasMaxLength(200);
        builder.Property(p => p.GatewayResponse).HasMaxLength(2000);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.RefundedAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.Method).HasConversion<int>();

        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => new { p.TenantId, p.Status });
    }
}
