using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Options;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    [Range(1, 365)] public int ExpiryDays { get; init; } = 7;
}
