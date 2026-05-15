using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Options;

public sealed class AccountLockoutOptions
{
    public const string SectionName = "AccountLockout";

    [Range(1, 100)] public int MaxFailedAttempts { get; init; } = 5;
    [Range(1, 1440)] public int LockoutDurationMinutes { get; init; } = 15;
}
