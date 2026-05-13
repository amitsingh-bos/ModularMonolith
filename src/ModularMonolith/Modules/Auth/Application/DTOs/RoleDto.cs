namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions);
