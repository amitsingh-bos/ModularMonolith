using ModularMonolith.BuildingBlocks.Domain.Primitives;

namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class Permission : Entity
{
    private Permission() { }

    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;

    public static Permission Create(string code, string description, string module) => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Description = description,
        Module = module
    };
}
