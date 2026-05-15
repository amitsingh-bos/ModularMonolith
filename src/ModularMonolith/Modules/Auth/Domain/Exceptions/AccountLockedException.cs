using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Auth.Domain.Exceptions;

public sealed class AccountLockedException : DomainException
{
    public DateTime LockoutEnd { get; }

    public AccountLockedException(DateTime lockoutEnd)
        : base($"Account locked until {lockoutEnd:u} due to too many failed login attempts.")
    {
        LockoutEnd = lockoutEnd;
    }
}
