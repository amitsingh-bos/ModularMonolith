using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Auth.Domain.Exceptions;

public sealed class TenantInactiveException : DomainException
{
    public TenantInactiveException()
        : base("The tenant account is inactive.") { }
}
