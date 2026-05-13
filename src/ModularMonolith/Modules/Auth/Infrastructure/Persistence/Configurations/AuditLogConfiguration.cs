using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TableName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.OldValues).HasColumnType("jsonb");
        builder.Property(a => a.NewValues).HasColumnType("jsonb");
        builder.Property(a => a.IpAddress).HasMaxLength(45);

        builder.HasIndex(a => new { a.TableName, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);

        builder.ToTable(t => t.HasCheckConstraint("ck_audit_logs_action",
            "action IN ('Created', 'Updated', 'Deleted')"));
    }
}
