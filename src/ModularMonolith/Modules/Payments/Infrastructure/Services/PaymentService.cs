using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Payments.Application.DTOs;
using ModularMonolith.Modules.Payments.Application.Services;
using ModularMonolith.Modules.Payments.Domain.Entities;
using ModularMonolith.Modules.Payments.Domain.Exceptions;
using ModularMonolith.Modules.Payments.Domain.Repositories;
using ModularMonolith.Modules.Payments.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Payments.Infrastructure.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentUser _currentUser;
    private readonly PaymentsDbContext _context;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ICurrentUser currentUser,
        PaymentsDbContext context)
    {
        _paymentRepository = paymentRepository;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<PaymentDto> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
            ?? throw new PaymentNotFoundException(paymentId);

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, ct);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(Guid tenantId, GetPaymentsRequest request, CancellationToken ct = default)
    {
        var (items, totalCount) = await _paymentRepository.GetPagedAsync(tenantId, request, ct);
        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<PaymentDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<PaymentDto> ProcessAsync(ProcessPaymentRequest request, CancellationToken ct = default)
    {
        var payment = Payment.Create(
            request.TenantId,
            request.OrderId,
            request.Amount,
            request.Currency,
            request.Method,
            request.TransactionReference,
            request.Notes);

        await _paymentRepository.AddAsync(payment, ct);
        await _paymentRepository.SaveChangesAsync(ct);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> CompleteAsync(Guid paymentId, string? transactionReference, string? gatewayResponse, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
            ?? throw new PaymentNotFoundException(paymentId);

        payment.Complete(transactionReference, gatewayResponse);
        _paymentRepository.Update(payment);
        await _paymentRepository.SaveChangesAsync(ct);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> FailAsync(Guid paymentId, string failureReason, string? gatewayResponse, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
            ?? throw new PaymentNotFoundException(paymentId);

        payment.Fail(failureReason, gatewayResponse);
        _paymentRepository.Update(payment);
        await _paymentRepository.SaveChangesAsync(ct);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> RefundAsync(Guid paymentId, RefundPaymentRequest request, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
            ?? throw new PaymentNotFoundException(paymentId);

        payment.Refund(request.RefundAmount, request.Notes);
        _paymentRepository.Update(payment);
        await _paymentRepository.SaveChangesAsync(ct);

        return MapToDto(payment);
    }

    private static PaymentDto MapToDto(Payment p) => new(
        p.Id,
        p.TenantId,
        p.OrderId,
        p.Amount,
        p.Currency,
        p.Method.ToString(),
        p.Status.ToString(),
        p.TransactionReference,
        p.FailureReason,
        p.Notes,
        p.ProcessedAt,
        p.RefundedAt,
        p.RefundedAmount,
        p.CreatedAt,
        p.UpdatedAt);
}
