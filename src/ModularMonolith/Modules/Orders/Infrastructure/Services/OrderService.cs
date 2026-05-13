using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Orders.Application.DTOs;
using ModularMonolith.Modules.Orders.Application.Services;
using ModularMonolith.Modules.Orders.Domain.Entities;
using ModularMonolith.Modules.Orders.Domain.Exceptions;
using ModularMonolith.Modules.Orders.Domain.Repositories;
using ModularMonolith.Modules.Orders.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Orders.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly OrdersDbContext _context;

    public OrderService(
        IOrderRepository orderRepository,
        ICurrentUser currentUser,
        OrdersDbContext context)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<OrderDto> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw new OrderNotFoundException(orderId);

        return MapToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(Guid tenantId, GetOrdersRequest request, CancellationToken ct = default)
    {
        var (items, totalCount) = await _orderRepository.GetPagedAsync(tenantId, request, ct);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<OrderDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var order = Order.Create(
            request.TenantId,
            request.CustomerId,
            request.ShippingAddressLine1,
            request.ShippingAddressLine2,
            request.ShippingCity,
            request.ShippingCountry,
            request.ShippingPostalCode,
            request.Notes);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.ProductSku, item.UnitPrice, item.Quantity);
        }

        await _orderRepository.AddAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        var created = await _orderRepository.GetByIdAsync(order.Id, ct);
        return MapToDto(created!);
    }

    public async Task<OrderDto> ConfirmAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw new OrderNotFoundException(orderId);

        order.Confirm();
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        return MapToDto(order);
    }

    public async Task<OrderDto> CancelAsync(Guid orderId, CancelOrderRequest request, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw new OrderNotFoundException(orderId);

        order.Cancel(request.Reason);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        return MapToDto(order);
    }

    public async Task<OrderDto> ShipAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw new OrderNotFoundException(orderId);

        order.Ship();
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        return MapToDto(order);
    }

    public async Task<OrderDto> DeliverAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw new OrderNotFoundException(orderId);

        order.Deliver();
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id,
        o.TenantId,
        o.CustomerId,
        o.OrderNumber,
        o.Status.ToString(),
        o.ShippingAddressLine1,
        o.ShippingAddressLine2,
        o.ShippingCity,
        o.ShippingCountry,
        o.ShippingPostalCode,
        o.Notes,
        o.TotalAmount,
        o.Items.Select(i => new OrderItemDto(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.ProductSku,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice)).ToList(),
        o.CreatedAt,
        o.UpdatedAt);
}
