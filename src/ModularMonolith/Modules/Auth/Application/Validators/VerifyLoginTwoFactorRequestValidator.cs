using FluentValidation;
using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Validators;

public sealed class VerifyLoginTwoFactorRequestValidator : AbstractValidator<VerifyLoginTwoFactorRequest>
{
    public VerifyLoginTwoFactorRequestValidator()
    {
        RuleFor(x => x.TwoFactorToken).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$")
            .WithMessage("Code must be a 6-digit number.");
    }
}
