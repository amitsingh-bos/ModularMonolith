namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class RolePermission
{
    private RolePermission() { }

    public Guid RoleId { get; private init; }
    public Guid PermissionId { get; private init; }

    public Role Role { get; private init; } = null!;
    public Permission Permission { get; private init; } = null!;

    public static RolePermission Create(Guid roleId, Guid permissionId) => new()
    {
        RoleId = roleId,
        PermissionId = permissionId
    };
}
