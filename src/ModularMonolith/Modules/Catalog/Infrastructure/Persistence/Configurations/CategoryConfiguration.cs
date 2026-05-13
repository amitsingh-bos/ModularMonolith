using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Catalog.Domain.Entities;

namespace ModularMonolith.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", "catalog");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasIndex(c => new { c.Slug, c.TenantId }).IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
