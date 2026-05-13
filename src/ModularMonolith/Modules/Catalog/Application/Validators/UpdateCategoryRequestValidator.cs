using FluentValidation;
using ModularMonolith.Modules.Catalog.Application.DTOs;

namespace ModularMonolith.Modules.Catalog.Application.Validators;

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
