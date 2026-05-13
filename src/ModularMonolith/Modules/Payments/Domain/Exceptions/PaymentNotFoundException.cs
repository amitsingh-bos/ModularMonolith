using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Payments.Domain.Exceptions;

public sealed class PaymentNotFoundException : NotFoundException
{
    public PaymentNotFoundException(Guid paymentId)
        : base("Payment", paymentId) { }
}
