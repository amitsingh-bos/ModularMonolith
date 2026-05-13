using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Orders.Domain.Entities;

namespace ModularMonolith.Modules.Orders.Application.DTOs;

public sealed class GetOrdersRequest : PagedQuery
{
    public OrderStatus? Status { get; init; }
    public Guid? CustomerId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
