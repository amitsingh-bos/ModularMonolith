namespace ModularMonolith.Modules.Orders.Application.DTOs;

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    string ProductSku,
    decimal UnitPrice,
    int Quantity);
