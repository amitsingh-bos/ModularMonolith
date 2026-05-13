namespace ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsResolved { get; }
    void SetTenant(Guid tenantId);
}

public sealed class TenantContext : ITenantContext
{
    private Guid _tenantId;
    private bool _isResolved;

    public Guid TenantId => _isResolved
        ? _tenantId
        : throw new InvalidOperationException("Tenant context has not been resolved.");

    public bool IsResolved => _isResolved;

    public void SetTenant(Guid tenantId)
    {
        _tenantId = tenantId;
        _isResolved = true;
    }
}
