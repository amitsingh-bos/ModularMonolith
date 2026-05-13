using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Payments.Application.DTOs;
using ModularMonolith.Modules.Payments.Domain.Entities;
using ModularMonolith.Modules.Payments.Domain.Repositories;
using ModularMonolith.Modules.Payments.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Payments.Infrastructure.Repositories;

public sealed class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
{
    private readonly PaymentsDbContext _context;

    public PaymentRepository(PaymentsDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

    public async Task<(IReadOnlyList<Payment> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId,
        GetPaymentsRequest request,
        CancellationToken ct = default)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.OrderId.HasValue)
            query = query.Where(p => p.OrderId == request.OrderId.Value);

        if (request.Method.HasValue)
            query = query.Where(p => p.Method == request.Method.Value);

        if (request.FromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(p => p.CreatedAt <= request.ToDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Payment payment, CancellationToken ct = default) =>
        await AddEntityAsync(payment, ct);

    public void Update(Payment payment) => UpdateEntity(payment);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
