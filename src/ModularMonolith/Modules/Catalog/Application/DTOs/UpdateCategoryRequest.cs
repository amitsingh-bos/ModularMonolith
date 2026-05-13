namespace ModularMonolith.Modules.Catalog.Application.DTOs;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId);
