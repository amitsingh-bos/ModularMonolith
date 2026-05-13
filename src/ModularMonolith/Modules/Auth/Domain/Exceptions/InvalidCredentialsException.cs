using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Auth.Domain.Exceptions;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid email or password.") { }
}
