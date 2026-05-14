using Microsoft.Extensions.Options;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class SmsService : ISmsService
{
    private readonly SmsOptions _options;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IOptions<SmsOptions> options, ILogger<SmsService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendOtpAsync(string phoneNumber, string code, CancellationToken ct = default)
    {
        if (_options.Provider == "Console" || string.IsNullOrEmpty(_options.AccountSid))
        {
            // Dev fallback — OTP visible in logs
            _logger.LogWarning("SMS provider not configured. [DEV] 2FA OTP for {PhoneNumber}: {Code}", phoneNumber, code);
            return Task.CompletedTask;
        }

        // TODO: plug in Twilio / AWS SNS / etc. when Provider is configured
        _logger.LogWarning(
            "SMS provider '{Provider}' is set but not yet implemented. [DEV] OTP for {PhoneNumber}: {Code}",
            _options.Provider, phoneNumber, code);

        return Task.CompletedTask;
    }
}
