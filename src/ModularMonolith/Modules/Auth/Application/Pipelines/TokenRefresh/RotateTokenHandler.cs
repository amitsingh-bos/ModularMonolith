using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.TokenRefresh;

public sealed class RotateTokenHandler : ChainHandlerBase<TokenRefreshContext>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ITokenService _tokenService;
    private readonly RefreshTokenOptions _options;

    public RotateTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ITokenService tokenService,
        IOptions<RefreshTokenOptions> options)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _tokenService = tokenService;
        _options = options.Value;
    }

    public override async Task HandleAsync(TokenRefreshContext context, CancellationToken ct)
    {
        if (context.StoredToken is null)
            throw new InvalidOperationException("Stored token must be set before rotation.");

        var rawNewToken = _tokenService.GenerateRefreshToken();
        var newHash = _tokenService.HashToken(rawNewToken);

        context.StoredToken.Revoke(newHash);
        _refreshTokenRepository.Update(context.StoredToken);

        var user = await _userRepository.GetByIdAsync(context.StoredToken.UserId, ct)
                   ?? throw new InvalidOperationException("User not found for token.");

        var newToken = RefreshToken.Create(
            user.Id,
            newHash,
            DateTime.UtcNow.AddDays(_options.ExpiryDays),
            context.StoredToken.DeviceInfo,
            context.IpAddress ?? context.StoredToken.IpAddress);

        await _refreshTokenRepository.AddAsync(newToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        var allRoles = await _roleRepository.GetAllAsync(user.TenantId, ct);
        var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        var userRoles = allRoles.Where(r => userRoleIds.Contains(r.Id)).ToList();

        context.User = user;
        context.Roles = userRoles.Select(r => r.Name).ToList();
        context.Permissions = userRoles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission?.Code ?? string.Empty)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        // Pass raw new token downstream for JWT generation
        context.StoredToken = newToken;
        // Temporarily store the raw token via a second property isn't possible on the immutable entity,
        // so we store it in context via a helper field
        context.NewRawToken = rawNewToken;

        await NextAsync(context, ct);
    }
}
