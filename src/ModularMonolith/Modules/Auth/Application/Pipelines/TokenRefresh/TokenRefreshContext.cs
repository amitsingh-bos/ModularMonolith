using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.TokenRefresh;

public sealed class TokenRefreshContext
{
    public string RawToken { get; init; } = string.Empty;
    public string? IpAddress { get; init; }

    public RefreshToken? StoredToken { get; set; }
    public User? User { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyList<string> Permissions { get; set; } = [];
    public TokenResponseDto? Result { get; set; }
    public string? NewRawToken { get; set; }
}
