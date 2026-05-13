namespace ModularMonolith.Modules.Catalog.Application.DTOs;

public sealed record CreateCategoryRequest(
    Guid TenantId,
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null);
