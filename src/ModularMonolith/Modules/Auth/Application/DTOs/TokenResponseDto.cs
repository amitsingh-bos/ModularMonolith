namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed class TokenResponseDto
{
    public TokenResponseDto(string accessToken, string refreshToken, int expiresIn)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresIn = expiresIn;
    }

    private TokenResponseDto() { }

    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public int ExpiresIn { get; init; }

    // Populated when 2FA is required; null on a normal successful login
    public bool RequiresTwoFactor { get; init; }
    public string? TwoFactorToken { get; init; }
    public string? TwoFactorMethod { get; init; }

    public static TokenResponseDto TwoFactorChallenge(string stepUpToken, string method) => new()
    {
        RequiresTwoFactor = true,
        TwoFactorToken = stepUpToken,
        TwoFactorMethod = method
    };
}
