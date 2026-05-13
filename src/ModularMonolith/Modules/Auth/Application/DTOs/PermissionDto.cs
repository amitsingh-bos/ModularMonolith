namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record PermissionDto(
    Guid Id,
    string Code,
    string Description,
    string Module);
