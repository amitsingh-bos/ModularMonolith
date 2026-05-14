namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed class TwoFactorStatusDto
{
    public bool Enabled { get; init; }
    public string? Method { get; init; }
}
