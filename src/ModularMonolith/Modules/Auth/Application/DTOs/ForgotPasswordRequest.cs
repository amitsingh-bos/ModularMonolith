namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record ForgotPasswordRequest(
    Guid TenantId,
    string Email,
    string Method);
