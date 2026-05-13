using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Exceptions;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.TokenRefresh;

public sealed class CheckRevocationHandler : ChainHandlerBase<TokenRefreshContext>
{
    public override async Task HandleAsync(TokenRefreshContext context, CancellationToken ct)
    {
        if (context.StoredToken is null || !context.StoredToken.IsActive)
            throw new InvalidTokenException();

        await NextAsync(context, ct);
    }
}
