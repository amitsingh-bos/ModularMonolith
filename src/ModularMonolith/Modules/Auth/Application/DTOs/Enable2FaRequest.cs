namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record Enable2FaRequest(
    string Method,          // "GoogleAuthenticator" | "Email" | "Sms"
    string? PhoneNumber);   // required only when Method = "Sms"
