using FluentValidation;
using ModularMonolith.Modules.Orders.Application.DTOs;

namespace ModularMonolith.Modules.Orders.Application.Validators;

public sealed class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductSku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
