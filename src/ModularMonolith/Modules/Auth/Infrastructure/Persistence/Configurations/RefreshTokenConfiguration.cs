using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.TokenHash).HasMaxLength(256).IsRequired();
        builder.Property(rt => rt.DeviceInfo).HasMaxLength(500);
        builder.Property(rt => rt.IpAddress).HasMaxLength(45);
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(256);
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => rt.UserId);
    }
}
