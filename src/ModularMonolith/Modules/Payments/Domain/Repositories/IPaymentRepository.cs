using ModularMonolith.Modules.Payments.Application.DTOs;
using ModularMonolith.Modules.Payments.Domain.Entities;

namespace ModularMonolith.Modules.Payments.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<(IReadOnlyList<Payment> Items, int TotalCount)> GetPagedAsync(Guid tenantId, GetPaymentsRequest request, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    void Update(Payment payment);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
