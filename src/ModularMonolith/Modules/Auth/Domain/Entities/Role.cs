using ModularMonolith.BuildingBlocks.Domain.Primitives;

namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class Role : Entity
{
    private readonly List<RolePermission> _rolePermissions = [];

    private Role() { }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public static Role Create(Guid tenantId, string name, string? description = null, bool isSystemRole = false) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Name = name,
        Description = description,
        IsSystemRole = isSystemRole
    };

    public void AddPermission(Guid permissionId)
    {
        if (_rolePermissions.All(rp => rp.PermissionId != permissionId))
            _rolePermissions.Add(RolePermission.Create(Id, permissionId));
    }

    public void RemovePermission(Guid permissionId)
    {
        var existing = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (existing is not null) _rolePermissions.Remove(existing);
    }
}
