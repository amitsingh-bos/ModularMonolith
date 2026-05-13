using ModularMonolith.Modules.Payments.Domain.Enums;

namespace ModularMonolith.Modules.Payments.Application.DTOs;

public sealed record ProcessPaymentRequest(
    Guid TenantId,
    Guid OrderId,
    decimal Amount,
    string Currency = "USD",
    PaymentMethod Method = PaymentMethod.CreditCard,
    string? TransactionReference = null,
    string? GatewayResponse = null,
    string? Notes = null);
