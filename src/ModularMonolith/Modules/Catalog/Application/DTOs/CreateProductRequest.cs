namespace ModularMonolith.Modules.Catalog.Application.DTOs;

public sealed record CreateProductRequest(
    Guid TenantId,
    Guid CategoryId,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    int StockQuantity = 0);
