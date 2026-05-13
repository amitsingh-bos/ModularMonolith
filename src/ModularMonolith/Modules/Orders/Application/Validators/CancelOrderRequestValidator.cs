using FluentValidation;
using ModularMonolith.Modules.Orders.Application.DTOs;

namespace ModularMonolith.Modules.Orders.Application.Validators;

public sealed class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
