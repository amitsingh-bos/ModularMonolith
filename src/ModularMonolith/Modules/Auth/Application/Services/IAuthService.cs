using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Services;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<TokenResponseDto> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task RevokeTokenAsync(RevokeTokenRequest request, CancellationToken ct = default);
}
