namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);
