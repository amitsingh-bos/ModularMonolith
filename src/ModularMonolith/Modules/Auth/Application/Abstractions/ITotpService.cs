namespace ModularMonolith.Modules.Auth.Application.Abstractions;

public interface ITotpService
{
    string GenerateSecretKey();
    string GetQrCodeUri(string email, string secretKey, string issuer);
    bool ValidateCode(string secretKey, string code);
}
