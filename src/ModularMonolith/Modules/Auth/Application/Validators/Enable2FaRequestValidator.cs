using FluentValidation;
using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Validators;

public sealed class Enable2FaRequestValidator : AbstractValidator<Enable2FaRequest>
{
    private static readonly string[] ValidMethods = ["GoogleAuthenticator", "Email", "Sms"];

    public Enable2FaRequestValidator()
    {
        RuleFor(x => x.Method)
            .NotEmpty()
            .Must(m => ValidMethods.Contains(m))
            .WithMessage("Method must be one of: GoogleAuthenticator, Email, Sms.");

        When(x => x.Method == "Sms", () =>
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required for SMS authentication.")
                .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("PhoneNumber must be a valid international phone number."));
    }
}
