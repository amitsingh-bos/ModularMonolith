using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Auth.Domain.Exceptions;

public sealed class InvalidTokenException : DomainException
{
    public InvalidTokenException()
        : base("The token is invalid, expired, or has been revoked.") { }
}
