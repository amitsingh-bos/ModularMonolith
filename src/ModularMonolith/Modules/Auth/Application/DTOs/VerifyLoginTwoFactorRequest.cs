namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed record VerifyLoginTwoFactorRequest(string TwoFactorToken, string Code);
