namespace ModularMonolith.Modules.Payments.Application.DTOs;

public sealed record PaymentDto(
    Guid Id,
    Guid TenantId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string? TransactionReference,
    string? FailureReason,
    string? Notes,
    DateTime? ProcessedAt,
    DateTime? RefundedAt,
    decimal? RefundedAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
