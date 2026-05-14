using OtpNet;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using System.Security.Cryptography;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class TotpService : ITotpService
{
    public string GenerateSecretKey()
    {
        var bytes = new byte[20];
        RandomNumberGenerator.Fill(bytes);
        return Base32Encoding.ToString(bytes);
    }

    public string GetQrCodeUri(string email, string secretKey, string issuer)
    {
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedIssuer = Uri.EscapeDataString(issuer);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool ValidateCode(string secretKey, string code)
    {
        try
        {
            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes);
            // allow 1 time-step window (±30 s) to handle clock drift
            return totp.VerifyTotp(DateTime.UtcNow, code, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }
}
