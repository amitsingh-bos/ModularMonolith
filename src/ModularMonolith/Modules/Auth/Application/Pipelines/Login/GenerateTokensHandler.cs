using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class GenerateTokensHandler : ChainHandlerBase<LoginContext>
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly RefreshTokenOptions _options;

    public GenerateTokensHandler(
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<RefreshTokenOptions> options)
    {
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _options = options.Value;
    }

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        if (context.User is null)
            throw new InvalidOperationException("User must be set before generating tokens.");

        var accessToken = _tokenService.GenerateAccessToken(context.User, context.Roles, context.Permissions);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var tokenHash = _tokenService.HashToken(rawRefreshToken);

        var refreshToken = RefreshToken.Create(
            context.User.Id,
            tokenHash,
            DateTime.UtcNow.AddDays(_options.ExpiryDays),
            context.DeviceInfo,
            context.IpAddress);

        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        context.Result = new TokenResponseDto(accessToken, rawRefreshToken, _options.ExpiryDays * 86400);
    }
}
