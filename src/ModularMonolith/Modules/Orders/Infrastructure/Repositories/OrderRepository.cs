using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Orders.Application.DTOs;
using ModularMonolith.Modules.Orders.Domain.Entities;
using ModularMonolith.Modules.Orders.Domain.Repositories;
using ModularMonolith.Modules.Orders.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Orders.Infrastructure.Repositories;

public sealed class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId,
        GetOrdersRequest request,
        CancellationToken ct = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.TenantId == tenantId);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToUpperInvariant();
            query = query.Where(o => o.OrderNumber.ToUpper().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await AddEntityAsync(order, ct);

    public void Update(Order order) => UpdateEntity(order);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
