using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.Modules.Payments.Domain.Enums;

namespace ModularMonolith.Modules.Payments.Domain.Exceptions;

public sealed class PaymentInvalidStateException : DomainException
{
    public PaymentInvalidStateException(Guid paymentId, string action, PaymentStatus currentStatus)
        : base($"Payment '{paymentId}' cannot be {action} because it is in {currentStatus} state.") { }
}
