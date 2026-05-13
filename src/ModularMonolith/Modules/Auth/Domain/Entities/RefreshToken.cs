using ModularMonolith.BuildingBlocks.Domain.Primitives;

namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    public Guid UserId { get; private init; }
    public string TokenHash { get; private init; } = string.Empty;
    public DateTime ExpiresAt { get; private init; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? DeviceInfo { get; private init; }
    public string? IpAddress { get; private init; }
    public DateTime CreatedAt { get; private init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? deviceInfo = null,
        string? ipAddress = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TokenHash = tokenHash,
        ExpiresAt = expiresAt,
        IsRevoked = false,
        DeviceInfo = deviceInfo,
        IpAddress = ipAddress,
        CreatedAt = DateTime.UtcNow
    };

    public void Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
