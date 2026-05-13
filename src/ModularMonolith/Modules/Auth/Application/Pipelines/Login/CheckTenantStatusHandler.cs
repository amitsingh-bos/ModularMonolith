using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class CheckTenantStatusHandler : ChainHandlerBase<LoginContext>
{
    private readonly ITenantRepository _tenantRepository;

    public CheckTenantStatusHandler(ITenantRepository tenantRepository)
        => _tenantRepository = tenantRepository;

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(context.TenantId, ct);

        if (tenant is null || !tenant.IsActive)
            throw new TenantInactiveException();

        context.Tenant = tenant;
        await NextAsync(context, ct);
    }
}
