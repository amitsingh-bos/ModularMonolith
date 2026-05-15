using System.Security.Cryptography;
using System.Text;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Enums;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITwoFactorTokenRepository _twoFactorTokenRepository;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITotpService _totpService;

    public PasswordResetService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITwoFactorTokenRepository twoFactorTokenRepository,
        IEmailService emailService,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        ITotpService totpService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _twoFactorTokenRepository = twoFactorTokenRepository;
        _emailService = emailService;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _totpService = totpService;
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var method = request.Method.ToLowerInvariant();

        if (method == "email")
            return await HandleEmailForgotPasswordAsync(request, ct);

        if (method == "totp")
            return await HandleTotpForgotPasswordAsync(request, ct);

        throw new DomainException($"Unsupported password reset method: {request.Method}.");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var method = request.Method.ToLowerInvariant();

        if (method == "email")
        {
            await HandleEmailResetPasswordAsync(request, ct);
            return;
        }

        if (method == "totp")
        {
            await HandleTotpResetPasswordAsync(request, ct);
            return;
        }

        throw new DomainException($"Unsupported password reset method: {request.Method}.");
    }

    // ── Email flow ─────────────────────────────────────────────────────────────

    private async Task<ForgotPasswordResponse> HandleEmailForgotPasswordAsync(
        ForgotPasswordRequest request, CancellationToken ct)
    {
        const string genericMessage = "If an account exists with that email address, you will receive a password reset email shortly.";

        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId, ct);
        if (user is null)
            return new ForgotPasswordResponse(genericMessage);

        // Invalidate any existing active reset tokens by simply creating a new one
        // (GetByHashAsync will still find the latest valid one on reset)
        var rawToken = GenerateSecureToken();
        var tokenHash = HashToken(rawToken);

        var resetToken = TwoFactorToken.Create(
            userId: user.Id,
            codeHash: tokenHash,
            method: TwoFactorMethod.Email,
            purpose: TwoFactorPurpose.PasswordReset,
            expiryMinutes: 15);

        await _twoFactorTokenRepository.AddAsync(resetToken, ct);
        await _twoFactorTokenRepository.SaveChangesAsync(ct);

        await _emailService.SendPasswordResetAsync(user.Email.Value, rawToken, ct);

        return new ForgotPasswordResponse(genericMessage);
    }

    private async Task HandleEmailResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email!, request.TenantId!.Value, ct)
            ?? throw new InvalidTokenException();

        var tokenHash = HashToken(request.ResetToken!);
        var storedToken = await _twoFactorTokenRepository.GetByHashAsync(
            user.Id, tokenHash, TwoFactorPurpose.PasswordReset, ct)
            ?? throw new InvalidTokenException();

        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newHash);
        _userRepository.Update(user);

        storedToken.MarkUsed();
        _twoFactorTokenRepository.Update(storedToken);
        await _twoFactorTokenRepository.SaveChangesAsync(ct);

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);
    }

    // ── TOTP flow ──────────────────────────────────────────────────────────────

    private async Task<ForgotPasswordResponse> HandleTotpForgotPasswordAsync(
        ForgotPasswordRequest request, CancellationToken ct)
    {
        const string genericMessage = "If an account with TOTP authentication exists, a step-up token has been issued. Submit your authenticator code to complete the reset.";

        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId, ct);
        if (user is null || !user.TwoFactorEnabled || user.TwoFactorMethod != TwoFactorMethod.GoogleAuthenticator)
            return new ForgotPasswordResponse(genericMessage);

        var stepUpToken = _tokenService.GeneratePasswordResetStepUpToken(user.Id);
        return new ForgotPasswordResponse(genericMessage, stepUpToken);
    }

    private async Task HandleTotpResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var userId = _tokenService.ValidatePasswordResetStepUpToken(request.StepUpToken!)
            ?? throw new InvalidTokenException();

        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidTokenException();

        if (!user.TwoFactorEnabled || user.TwoFactorMethod != TwoFactorMethod.GoogleAuthenticator)
            throw new InvalidTokenException();

        var valid = _totpService.ValidateCode(user.TwoFactorSecretKey!, request.TotpCode!);
        if (!valid)
            throw new InvalidTokenException();

        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newHash);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static string GenerateSecureToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
