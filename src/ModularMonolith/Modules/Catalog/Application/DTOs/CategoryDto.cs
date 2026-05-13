namespace ModularMonolith.Modules.Catalog.Application.DTOs;

public sealed record CategoryDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive,
    DateTime CreatedAt);
