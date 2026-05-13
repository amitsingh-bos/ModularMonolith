namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record LoginRequest(
    Guid TenantId,
    string Email,
    string Password,
    string? DeviceInfo = null);
