using System.Security.Cryptography;
using System.Text;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Enums;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class Check2FaHandler : ChainHandlerBase<LoginContext>
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ITwoFactorTokenRepository _twoFactorTokenRepository;

    public Check2FaHandler(
        ITokenService tokenService,
        IEmailService emailService,
        ISmsService smsService,
        ITwoFactorTokenRepository twoFactorTokenRepository)
    {
        _tokenService = tokenService;
        _emailService = emailService;
        _smsService = smsService;
        _twoFactorTokenRepository = twoFactorTokenRepository;
    }

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        var user = context.User!;

        if (!user.TwoFactorEnabled)
        {
            await NextAsync(context, ct);
            return;
        }

        var method = user.TwoFactorMethod!.Value;

        if (method is TwoFactorMethod.Email or TwoFactorMethod.Sms)
        {
            var code = GenerateOtpCode();
            var codeHash = HashCode(code);
            var token = TwoFactorToken.Create(user.Id, codeHash, method, TwoFactorPurpose.Login);
            await _twoFactorTokenRepository.AddAsync(token, ct);
            await _twoFactorTokenRepository.SaveChangesAsync(ct);

            if (method == TwoFactorMethod.Email)
                await _emailService.SendOtpAsync(user.Email.Value, code, ct);
            else
                await _smsService.SendOtpAsync(user.PhoneNumber!, code, ct);
        }

        var stepUpToken = _tokenService.GenerateStepUpToken(user.Id, method.ToString());

        context.TwoFactorRequired = true;
        context.Result = TokenResponseDto.TwoFactorChallenge(stepUpToken, method.ToString());
        // pipeline stops here — GenerateTokensHandler is NOT called
    }

    private static string GenerateOtpCode() =>
        RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString("D6");

    private static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
