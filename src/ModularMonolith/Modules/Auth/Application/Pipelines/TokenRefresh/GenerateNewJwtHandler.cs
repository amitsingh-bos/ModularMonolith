using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.TokenRefresh;

public sealed class GenerateNewJwtHandler : ChainHandlerBase<TokenRefreshContext>
{
    private readonly ITokenService _tokenService;
    private readonly RefreshTokenOptions _refreshOptions;

    public GenerateNewJwtHandler(ITokenService tokenService, IOptions<RefreshTokenOptions> refreshOptions)
    {
        _tokenService = tokenService;
        _refreshOptions = refreshOptions.Value;
    }

    public override Task HandleAsync(TokenRefreshContext context, CancellationToken ct)
    {
        if (context.User is null || context.NewRawToken is null)
            throw new InvalidOperationException("User and new raw token must be set before generating JWT.");

        var accessToken = _tokenService.GenerateAccessToken(context.User, context.Roles, context.Permissions);
        context.Result = new TokenResponseDto(accessToken, context.NewRawToken, _refreshOptions.ExpiryDays * 86400);
        return Task.CompletedTask;
    }
}
