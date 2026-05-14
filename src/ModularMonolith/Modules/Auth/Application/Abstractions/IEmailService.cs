namespace ModularMonolith.Modules.Auth.Application.Abstractions;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string code, CancellationToken ct = default);
}
