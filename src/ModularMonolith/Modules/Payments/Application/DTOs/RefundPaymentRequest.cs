namespace ModularMonolith.Modules.Payments.Application.DTOs;

public sealed record RefundPaymentRequest(
    decimal RefundAmount,
    string? Notes = null);
