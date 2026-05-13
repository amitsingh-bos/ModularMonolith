namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record CreateRoleRequest(
    Guid TenantId,
    string Name,
    string? Description);
