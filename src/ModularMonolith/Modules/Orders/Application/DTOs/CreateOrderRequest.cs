namespace ModularMonolith.Modules.Orders.Application.DTOs;

public sealed record CreateOrderRequest(
    Guid TenantId,
    Guid CustomerId,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingCountry,
    string ShippingPostalCode,
    string? Notes,
    IReadOnlyList<CreateOrderItemRequest> Items);
