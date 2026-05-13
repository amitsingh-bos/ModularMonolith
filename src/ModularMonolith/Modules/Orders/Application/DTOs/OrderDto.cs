namespace ModularMonolith.Modules.Orders.Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    string OrderNumber,
    string Status,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingCountry,
    string ShippingPostalCode,
    string? Notes,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
