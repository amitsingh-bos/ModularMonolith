using Microsoft.Extensions.Options;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Pipelines.Login;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Application.Pipelines.TokenRefresh;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly RefreshTokenOptions _refreshOptions;
    private readonly ValidateCredentialsHandler _validateCredentials;
    private readonly CheckAccountStatusHandler _checkAccountStatus;
    private readonly CheckTenantStatusHandler _checkTenantStatus;
    private readonly RecordLoginAuditHandler _recordLoginAudit;
    private readonly Check2FaHandler _check2Fa;
    private readonly GenerateTokensHandler _generateTokens;
    private readonly LoadRefreshTokenHandler _loadRefreshToken;
    private readonly CheckRevocationHandler _checkRevocation;
    private readonly RotateTokenHandler _rotateToken;
    private readonly GenerateNewJwtHandler _generateNewJwt;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<RefreshTokenOptions> refreshOptions,
        ValidateCredentialsHandler validateCredentials,
        CheckAccountStatusHandler checkAccountStatus,
        CheckTenantStatusHandler checkTenantStatus,
        RecordLoginAuditHandler recordLoginAudit,
        Check2FaHandler check2Fa,
        GenerateTokensHandler generateTokens,
        LoadRefreshTokenHandler loadRefreshToken,
        CheckRevocationHandler checkRevocation,
        RotateTokenHandler rotateToken,
        GenerateNewJwtHandler generateNewJwt)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _tenantRepository = tenantRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshOptions = refreshOptions.Value;
        _validateCredentials = validateCredentials;
        _checkAccountStatus = checkAccountStatus;
        _checkTenantStatus = checkTenantStatus;
        _recordLoginAudit = recordLoginAudit;
        _check2Fa = check2Fa;
        _generateTokens = generateTokens;
        _loadRefreshToken = loadRefreshToken;
        _checkRevocation = checkRevocation;
        _rotateToken = rotateToken;
        _generateNewJwt = generateNewJwt;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // Auto-provision the tenant on first registration so it exists for the login pipeline
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            var slug = request.TenantId.ToString();
            tenant = Tenant.Create(slug, slug, request.TenantId);
            await _tenantRepository.AddAsync(tenant, ct);
            await _tenantRepository.SaveChangesAsync(ct);
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email, request.TenantId, ct))
            throw new UserAlreadyExistsException(request.Email);

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.TenantId, request.Email, hash, request.FirstName, request.LastName);

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        // First user in a tenant becomes Admin; all subsequent users get the User role
        var userCount = await _userRepository.CountByTenantAsync(request.TenantId, ct);
        var roleName = userCount == 1 ? "Admin" : "User";
        var permissionCodes = userCount == 1 ? Permissions.All : Permissions.UserDefault;

        var role = await _roleRepository.GetByNameAsync(roleName, request.TenantId, ct);
        if (role is null)
        {
            role = Role.Create(request.TenantId, roleName, isSystemRole: true);
            var perms = await _permissionRepository.GetByCodesAsync(permissionCodes, ct);
            foreach (var p in perms)
                role.AddPermission(p.Id);
            await _roleRepository.AddAsync(role, ct);
            await _roleRepository.SaveChangesAsync(ct);
        }

        user.AssignRole(role.Id);
        await _userRepository.SaveChangesAsync(ct);

        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var tokenHash = _tokenService.HashToken(rawRefreshToken);
        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenHash,
            DateTime.UtcNow.AddDays(_refreshOptions.ExpiryDays));

        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        var roleNames = new[] { roleName };
        var permissions = permissionCodes.ToArray();
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames, permissions);
        return new TokenResponseDto(accessToken, rawRefreshToken, _refreshOptions.ExpiryDays * 86400);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var context = new LoginContext
        {
            TenantId = request.TenantId,
            Email = request.Email,
            Password = request.Password,
            DeviceInfo = request.DeviceInfo,
            IpAddress = ipAddress
        };

        _validateCredentials
            .SetNext(_checkAccountStatus)
            .SetNext(_checkTenantStatus)
            .SetNext(_recordLoginAudit)
            .SetNext(_check2Fa)
            .SetNext(_generateTokens);

        await _validateCredentials.HandleAsync(context, ct);
        return context.Result!;
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var context = new TokenRefreshContext
        {
            RawToken = request.RefreshToken,
            IpAddress = ipAddress
        };

        _loadRefreshToken
            .SetNext(_checkRevocation)
            .SetNext(_rotateToken)
            .SetNext(_generateNewJwt);

        await _loadRefreshToken.HandleAsync(context, ct);
        return context.Result!;
    }

    public async Task RevokeTokenAsync(RevokeTokenRequest request, CancellationToken ct = default)
    {
        var hash = _tokenService.HashToken(request.RefreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(hash, ct);

        if (token is null || !token.IsActive)
            throw new InvalidTokenException();

        token.Revoke();
        _refreshTokenRepository.Update(token);
        await _refreshTokenRepository.SaveChangesAsync(ct);
    }
}