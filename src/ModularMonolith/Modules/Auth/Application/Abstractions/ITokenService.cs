using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Application.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    string HashToken(string token);

    // Short-lived step-up token issued when 2FA is required after password validation
    string GenerateStepUpToken(Guid userId, string twoFactorMethod);
    (Guid userId, string method)? ValidateStepUpToken(string token);
}
