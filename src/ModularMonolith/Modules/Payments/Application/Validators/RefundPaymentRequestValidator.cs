using FluentValidation;
using ModularMonolith.Modules.Payments.Application.DTOs;

namespace ModularMonolith.Modules.Payments.Application.Validators;

public sealed class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
{
    public RefundPaymentRequestValidator()
    {
        RuleFor(x => x.RefundAmount).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
