using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class RecordLoginAuditHandler : ChainHandlerBase<LoginContext>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public RecordLoginAuditHandler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        if (context.User is null)
            throw new InvalidOperationException("User must be set before recording login.");

        context.User.RecordLogin();

        var allRoles = await _roleRepository.GetAllAsync(context.TenantId, ct);
        var userRoleIds = context.User.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        var userRoles = allRoles.Where(r => userRoleIds.Contains(r.Id)).ToList();

        context.Roles = userRoles.Select(r => r.Name).ToList();
        context.Permissions = userRoles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission?.Code ?? string.Empty)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        _userRepository.Update(context.User);
        await _userRepository.SaveChangesAsync(ct);

        await NextAsync(context, ct);
    }
}
