using ModularMonolith.BuildingBlocks.Domain.Primitives;

namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class Tenant : Entity
{
    private Tenant() { }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private init; }

    public static Tenant Create(string name, string slug, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        Slug = slug.ToLowerInvariant(),
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
