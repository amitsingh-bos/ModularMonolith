using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;

namespace ModularMonolith.Modules.Catalog.Domain.Entities;

public sealed class Category : Entity, IAuditableEntity, ISoftDeletable
{
    private Category() { }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public static Category Create(Guid tenantId, string name, string? description = null, Guid? parentCategoryId = null) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Name = name,
        Slug = GenerateSlug(name),
        Description = description,
        ParentCategoryId = parentCategoryId,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public void Update(string name, string? description, Guid? parentCategoryId)
    {
        Name = name;
        Slug = GenerateSlug(name);
        Description = description;
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    private static string GenerateSlug(string name) =>
        name.ToLowerInvariant().Replace(" ", "-");
}
