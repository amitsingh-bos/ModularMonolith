namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record ResetPasswordRequest(
    string Method,
    string NewPassword,
    // Email-method fields
    Guid? TenantId = null,
    string? Email = null,
    string? ResetToken = null,
    // TOTP-method fields
    string? StepUpToken = null,
    string? TotpCode = null);
