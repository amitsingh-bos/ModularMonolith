using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Payments.Application.DTOs;

namespace ModularMonolith.Modules.Payments.Application.Services;

public interface IPaymentService
{
    Task<PaymentDto> GetByIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<PagedResult<PaymentDto>> GetPaymentsAsync(Guid tenantId, GetPaymentsRequest request, CancellationToken ct = default);
    Task<PaymentDto> ProcessAsync(ProcessPaymentRequest request, CancellationToken ct = default);
    Task<PaymentDto> CompleteAsync(Guid paymentId, string? transactionReference, string? gatewayResponse, CancellationToken ct = default);
    Task<PaymentDto> FailAsync(Guid paymentId, string failureReason, string? gatewayResponse, CancellationToken ct = default);
    Task<PaymentDto> RefundAsync(Guid paymentId, RefundPaymentRequest request, CancellationToken ct = default);
}
