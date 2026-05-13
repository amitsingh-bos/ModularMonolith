using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class CheckAccountStatusHandler : ChainHandlerBase<LoginContext>
{
    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        if (context.User is null)
            throw new InvalidOperationException("User must be set before account status check.");

        if (!context.User.IsActive)
            throw new DomainException("Your account has been deactivated. Please contact support.");

        await NextAsync(context, ct);
    }
}
