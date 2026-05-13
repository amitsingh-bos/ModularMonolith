using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.TokenRefresh;

public sealed class LoadRefreshTokenHandler : ChainHandlerBase<TokenRefreshContext>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public LoadRefreshTokenHandler(IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public override async Task HandleAsync(TokenRefreshContext context, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(context.RawToken))
            throw new InvalidTokenException();

        var tokenHash = _tokenService.HashToken(context.RawToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (stored is null)
            throw new InvalidTokenException();

        context.StoredToken = stored;
        await NextAsync(context, ct);
    }
}
