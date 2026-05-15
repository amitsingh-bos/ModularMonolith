using FluentValidation;
using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Validators;

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    private static readonly string[] ValidMethods = ["email", "totp"];

    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Method)
            .NotEmpty()
            .Must(m => ValidMethods.Contains(m?.ToLowerInvariant()))
            .WithMessage("Method must be 'email' or 'totp'.");
    }
}
