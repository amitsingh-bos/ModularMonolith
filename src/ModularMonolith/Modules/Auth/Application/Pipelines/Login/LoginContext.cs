using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class LoginContext
{
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? DeviceInfo { get; init; }
    public string? IpAddress { get; init; }

    public User? User { get; set; }
    public Tenant? Tenant { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyList<string> Permissions { get; set; } = [];
    public TokenResponseDto? Result { get; set; }

    public bool TwoFactorRequired { get; set; }
}
