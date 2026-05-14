namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed class Setup2FaResponseDto
{
    public string Method { get; init; } = string.Empty;

    // GoogleAuthenticator only — scan this in the app
    public string? QrCodeUri { get; init; }
    // GoogleAuthenticator only — for manual entry
    public string? SecretKey { get; init; }

    // Email / SMS — describes where the OTP was sent
    public string? Message { get; init; }
}
