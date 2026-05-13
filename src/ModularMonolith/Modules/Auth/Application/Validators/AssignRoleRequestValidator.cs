using FluentValidation;
using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Validators;

public sealed class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
{
    public AssignRoleRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
