using FluentValidation;
using ModularMonolith.Modules.Catalog.Application.DTOs;

namespace ModularMonolith.Modules.Catalog.Application.Validators;

public sealed class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("Stock delta cannot be zero.");
    }
}
