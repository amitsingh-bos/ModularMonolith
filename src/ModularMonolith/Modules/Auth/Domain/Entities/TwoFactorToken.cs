using ModularMonolith.Modules.Auth.Domain.Enums;

namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class TwoFactorToken
{
    private TwoFactorToken() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public TwoFactorMethod Method { get; private set; }
    public string Purpose { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsUsed && !IsExpired;

    public static TwoFactorToken Create(
        Guid userId,
        string codeHash,
        TwoFactorMethod method,
        string purpose,
        int expiryMinutes = 10) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CodeHash = codeHash,
        Method = method,
        Purpose = purpose,
        ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
        IsUsed = false,
        CreatedAt = DateTime.UtcNow
    };

    public void MarkUsed() => IsUsed = true;
}
