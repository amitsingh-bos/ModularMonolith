using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendOtpAsync(string toEmail, string code, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.SmtpHost))
        {
            // Dev fallback — OTP visible in logs so developers can test without SMTP
            _logger.LogWarning("SMTP not configured. [DEV] 2FA OTP for {Email}: {Code}", toEmail, code);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Your verification code";
        message.Body = new BodyBuilder
        {
            HtmlBody = $"""
                        <p>Your verification code is:</p>
                        <h2 style="letter-spacing:4px">{code}</h2>
                        <p>This code expires in <strong>10 minutes</strong>. Do not share it with anyone.</p>
                        """
        }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort,
            _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);

        if (!string.IsNullOrEmpty(_options.Username))
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(quit: true, ct);
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.SmtpHost))
        {
            _logger.LogWarning("SMTP not configured. [DEV] Password reset token for {Email}: {Token}", toEmail, resetToken);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Reset your password";
        message.Body = new BodyBuilder
        {
            HtmlBody = $"""
                        <p>We received a request to reset the password for your account.</p>
                        <p>Use the token below to reset your password. It expires in <strong>15 minutes</strong>.</p>
                        <h2 style="letter-spacing:2px;font-family:monospace">{resetToken}</h2>
                        <p>If you did not request a password reset, you can safely ignore this email.</p>
                        """
        }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort,
            _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);

        if (!string.IsNullOrEmpty(_options.Username))
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(quit: true, ct);
    }
}
