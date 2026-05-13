namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record RegisterRequest(
    Guid TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName);
