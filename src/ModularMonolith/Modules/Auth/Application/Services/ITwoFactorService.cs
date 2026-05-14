using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Domain.Enums;

namespace ModularMonolith.Modules.Auth.Application.Services;

public interface ITwoFactorService
{
    Task<Setup2FaResponseDto> SetupAsync(Guid userId, TwoFactorMethod method, string? phoneNumber, CancellationToken ct = default);
    Task ConfirmSetupAsync(Guid userId, string code, CancellationToken ct = default);
    Task DisableAsync(Guid userId, string code, CancellationToken ct = default);
    Task<TwoFactorStatusDto> GetStatusAsync(Guid userId, CancellationToken ct = default);
    Task<TokenResponseDto> VerifyLoginAsync(string stepUpToken, string code, string? ipAddress, CancellationToken ct = default);
}
