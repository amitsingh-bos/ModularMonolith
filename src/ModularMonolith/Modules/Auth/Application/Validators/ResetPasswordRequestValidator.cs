using FluentValidation;
using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        When(x => x.Method?.ToLowerInvariant() == "email", () =>
        {
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required for email-based reset.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required for email-based reset.");
            RuleFor(x => x.ResetToken).NotEmpty().WithMessage("ResetToken is required for email-based reset.");
        });

        When(x => x.Method?.ToLowerInvariant() == "totp", () =>
        {
            RuleFor(x => x.StepUpToken).NotEmpty().WithMessage("StepUpToken is required for TOTP-based reset.");
            RuleFor(x => x.TotpCode)
                .NotEmpty()
                .Matches(@"^\d{6}$")
                .WithMessage("TotpCode must be a 6-digit number.");
        });
    }
}
