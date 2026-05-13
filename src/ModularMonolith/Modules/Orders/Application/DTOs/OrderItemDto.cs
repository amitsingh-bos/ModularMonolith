namespace ModularMonolith.Modules.Orders.Application.DTOs;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
