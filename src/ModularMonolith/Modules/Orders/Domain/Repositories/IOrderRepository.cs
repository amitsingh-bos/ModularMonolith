using ModularMonolith.Modules.Orders.Application.DTOs;
using ModularMonolith.Modules.Orders.Domain.Entities;

namespace ModularMonolith.Modules.Orders.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(Guid tenantId, GetOrdersRequest request, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
