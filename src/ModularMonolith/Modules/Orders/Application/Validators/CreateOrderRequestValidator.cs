using FluentValidation;
using ModularMonolith.Modules.Orders.Application.DTOs;

namespace ModularMonolith.Modules.Orders.Application.Validators;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ShippingAddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ShippingCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingCountry).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingPostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}
