using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Infrastructure.Persistence.Configurations;

public sealed class TwoFactorTokenConfiguration : IEntityTypeConfiguration<TwoFactorToken>
{
    public void Configure(EntityTypeBuilder<TwoFactorToken> builder)
    {
        builder.ToTable("two_factor_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.CodeHash).HasMaxLength(64).IsRequired();
        builder.Property(t => t.Method).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(t => t.Purpose).HasMaxLength(20).IsRequired();
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.IsUsed).HasDefaultValue(false);
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => new { t.UserId, t.Method, t.Purpose, t.IsUsed });
    }
}
