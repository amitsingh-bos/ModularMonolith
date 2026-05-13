using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Auth.Domain.Exceptions;

public sealed class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException(string email)
        : base($"A user with email '{email}' already exists.") { }
}
