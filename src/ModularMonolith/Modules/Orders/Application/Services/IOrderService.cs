using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Orders.Application.DTOs;

namespace ModularMonolith.Modules.Orders.Application.Services;

public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetOrdersAsync(Guid tenantId, GetOrdersRequest request, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto> ConfirmAsync(Guid orderId, CancellationToken ct = default);
    Task<OrderDto> CancelAsync(Guid orderId, CancelOrderRequest request, CancellationToken ct = default);
    Task<OrderDto> ShipAsync(Guid orderId, CancellationToken ct = default);
    Task<OrderDto> DeliverAsync(Guid orderId, CancellationToken ct = default);
    Task MarkRefundedAsync(Guid orderId, CancellationToken ct = default);
}
