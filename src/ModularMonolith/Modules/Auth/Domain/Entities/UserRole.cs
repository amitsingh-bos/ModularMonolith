namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class UserRole
{
    private UserRole() { }

    public Guid UserId { get; private init; }
    public Guid RoleId { get; private init; }
    public DateTime AssignedAt { get; private init; }

    public User User { get; private init; } = null!;
    public Role Role { get; private init; } = null!;

    public static UserRole Create(Guid userId, Guid roleId) => new()
    {
        UserId = userId,
        RoleId = roleId,
        AssignedAt = DateTime.UtcNow
    };
}
