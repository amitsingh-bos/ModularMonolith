namespace ModularMonolith.Modules.Auth.Application.Abstractions;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string code, CancellationToken ct = default);
}
