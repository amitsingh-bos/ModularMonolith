using FluentValidation;
using ModularMonolith.Modules.Payments.Application.DTOs;
using ModularMonolith.Modules.Payments.Domain.Enums;

namespace ModularMonolith.Modules.Payments.Application.Validators;

public sealed class ProcessPaymentRequestValidator : AbstractValidator<ProcessPaymentRequest>
{
    public ProcessPaymentRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.Method).IsInEnum();
    }
}
