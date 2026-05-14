using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Enums;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class TwoFactorService : ITwoFactorService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITwoFactorTokenRepository _twoFactorTokenRepository;
    private readonly ITotpService _totpService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ITokenService _tokenService;
    private readonly RefreshTokenOptions _refreshOptions;
    private const string Issuer = "ModularMonolith";

    public TwoFactorService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITwoFactorTokenRepository twoFactorTokenRepository,
        ITotpService totpService,
        IEmailService emailService,
        ISmsService smsService,
        ITokenService tokenService,
        IOptions<RefreshTokenOptions> refreshOptions)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _twoFactorTokenRepository = twoFactorTokenRepository;
        _totpService = totpService;
        _emailService = emailService;
        _smsService = smsService;
        _tokenService = tokenService;
        _refreshOptions = refreshOptions.Value;
    }

    public async Task<Setup2FaResponseDto> SetupAsync(
        Guid userId, TwoFactorMethod method, string? phoneNumber, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        if (method == TwoFactorMethod.Sms && string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("PhoneNumber is required for SMS authentication.");

        if (method == TwoFactorMethod.GoogleAuthenticator)
        {
            var secretKey = _totpService.GenerateSecretKey();
            user.SetupTwoFactor(TwoFactorMethod.GoogleAuthenticator, secretKey: secretKey);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(ct);

            return new Setup2FaResponseDto
            {
                Method = "GoogleAuthenticator",
                QrCodeUri = _totpService.GetQrCodeUri(user.Email.Value, secretKey, Issuer),
                SecretKey = secretKey
            };
        }

        user.SetupTwoFactor(method, phoneNumber: phoneNumber);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        var code = GenerateOtpCode();
        var token = TwoFactorToken.Create(user.Id, HashCode(code), method, TwoFactorPurpose.Setup);
        await _twoFactorTokenRepository.AddAsync(token, ct);
        await _twoFactorTokenRepository.SaveChangesAsync(ct);

        if (method == TwoFactorMethod.Email)
        {
            await _emailService.SendOtpAsync(user.Email.Value, code, ct);
            return new Setup2FaResponseDto { Method = "Email", Message = $"OTP sent to {user.Email.Value}" };
        }

        await _smsService.SendOtpAsync(phoneNumber!, code, ct);
        return new Setup2FaResponseDto { Method = "Sms", Message = $"OTP sent to {phoneNumber}" };
    }

    public async Task ConfirmSetupAsync(Guid userId, string code, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        if (user.TwoFactorMethod is null)
            throw new DomainException("Two-factor setup has not been initiated.");

        var valid = user.TwoFactorMethod switch
        {
            TwoFactorMethod.GoogleAuthenticator => _totpService.ValidateCode(user.TwoFactorSecretKey!, code),
            _ => await ValidateOtpCodeAsync(user.Id, user.TwoFactorMethod.Value, TwoFactorPurpose.Setup, code, ct)
        };

        if (!valid)
            throw new InvalidTokenException();

        user.EnableTwoFactor();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
    }

    public async Task DisableAsync(Guid userId, string code, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        if (!user.TwoFactorEnabled)
            throw new DomainException("Two-factor authentication is not enabled.");

        var valid = user.TwoFactorMethod switch
        {
            TwoFactorMethod.GoogleAuthenticator => _totpService.ValidateCode(user.TwoFactorSecretKey!, code),
            _ => await ValidateOtpCodeAsync(user.Id, user.TwoFactorMethod!.Value, TwoFactorPurpose.Disable, code, ct)
        };

        if (!valid)
            throw new InvalidTokenException();

        user.DisableTwoFactor();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
    }

    public async Task<TwoFactorStatusDto> GetStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        return new TwoFactorStatusDto
        {
            Enabled = user.TwoFactorEnabled,
            Method = user.TwoFactorEnabled ? user.TwoFactorMethod?.ToString() : null
        };
    }

    public async Task<TokenResponseDto> VerifyLoginAsync(
        string stepUpToken, string code, string? ipAddress, CancellationToken ct = default)
    {
        var parsed = _tokenService.ValidateStepUpToken(stepUpToken)
            ?? throw new InvalidTokenException();

        var (userId, methodName) = parsed;

        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidTokenException();

        if (!user.TwoFactorEnabled)
            throw new DomainException("Two-factor authentication is not enabled for this account.");

        var valid = methodName switch
        {
            "GoogleAuthenticator" => _totpService.ValidateCode(user.TwoFactorSecretKey!, code),
            "Email" => await ValidateOtpCodeAsync(user.Id, TwoFactorMethod.Email, TwoFactorPurpose.Login, code, ct),
            "Sms" => await ValidateOtpCodeAsync(user.Id, TwoFactorMethod.Sms, TwoFactorPurpose.Login, code, ct),
            _ => false
        };

        if (!valid)
            throw new InvalidTokenException();

        var allRoles = await _roleRepository.GetAllAsync(user.TenantId, ct);
        var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        var userRoles = allRoles.Where(r => userRoleIds.Contains(r.Id)).ToList();
        var roles = userRoles.Select(r => r.Name).ToList();
        var permissions = userRoles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission?.Code ?? string.Empty)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var tokenHash = _tokenService.HashToken(rawRefreshToken);
        var refreshToken = RefreshToken.Create(
            user.Id, tokenHash,
            DateTime.UtcNow.AddDays(_refreshOptions.ExpiryDays),
            ipAddress: ipAddress);

        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        return new TokenResponseDto(accessToken, rawRefreshToken, _refreshOptions.ExpiryDays * 86400);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<bool> ValidateOtpCodeAsync(
        Guid userId, TwoFactorMethod method, string purpose, string code, CancellationToken ct)
    {
        var token = await _twoFactorTokenRepository.GetActiveAsync(userId, method, purpose, ct);
        if (token is null) return false;

        if (token.CodeHash != HashCode(code)) return false;

        token.MarkUsed();
        _twoFactorTokenRepository.Update(token);
        await _twoFactorTokenRepository.SaveChangesAsync(ct);
        return true;
    }

    private static string GenerateOtpCode() =>
        RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString("D6");

    private static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
