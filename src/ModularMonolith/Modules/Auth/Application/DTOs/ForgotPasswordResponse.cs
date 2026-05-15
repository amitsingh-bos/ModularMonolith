namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record ForgotPasswordResponse(
    string Message,
    string? StepUpToken = null);
