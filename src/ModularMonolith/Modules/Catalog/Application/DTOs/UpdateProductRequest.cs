namespace ModularMonolith.Modules.Catalog.Application.DTOs;

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId);
