namespace ModularMonolith.Modules.Catalog.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
