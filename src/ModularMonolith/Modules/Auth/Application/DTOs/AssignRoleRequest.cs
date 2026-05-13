namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record AssignRoleRequest(Guid UserId, Guid RoleId);
