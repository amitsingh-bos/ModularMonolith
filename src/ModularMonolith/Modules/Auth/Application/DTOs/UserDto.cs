namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    bool IsEmailVerified,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles);
