using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;
using ModularMonolith.Modules.Auth.Domain.Enums;
using ModularMonolith.Modules.Auth.Domain.Events;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.ValueObjects;

namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class User : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    private readonly List<UserRole> _userRoles = [];

    private User() { }

    public Guid TenantId { get; private set; }
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    public bool TwoFactorEnabled { get; private set; }
    public TwoFactorMethod? TwoFactorMethod { get; private set; }
    public string? TwoFactorSecretKey { get; private set; }
    public string? PhoneNumber { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static User Create(
        Guid tenantId,
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, tenantId, email));
        return user;
    }

    public bool IsLockedOut() => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserLoggedInDomainEvent(Id, TenantId));
    }

    public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
            LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            throw new DomainException($"Role '{roleId}' is already assigned to user '{Id}'.");

        _userRoles.Add(UserRole.Create(Id, roleId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(Guid roleId)
    {
        var existing = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (existing is null)
            throw new DomainException($"Role '{roleId}' is not assigned to user '{Id}'.");

        _userRoles.Remove(existing);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(Guid? deactivatedBy = null)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deactivatedBy;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void UpdatePassword(string newPasswordHash, Guid? updatedBy = null)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void SetupTwoFactor(TwoFactorMethod method, string? secretKey = null, string? phoneNumber = null)
    {
        TwoFactorMethod = method;
        TwoFactorSecretKey = secretKey;
        PhoneNumber = phoneNumber;
        TwoFactorEnabled = false; // enabled only after confirmation
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorMethod = null;
        TwoFactorSecretKey = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
