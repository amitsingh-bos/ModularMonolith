using Microsoft.Extensions.Options;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class CheckAccountLockoutHandler : ChainHandlerBase<LoginContext>
{
    private readonly IUserRepository _userRepository;
    private readonly AccountLockoutOptions _lockoutOptions;

    public CheckAccountLockoutHandler(IUserRepository userRepository, IOptions<AccountLockoutOptions> lockoutOptions)
    {
        _userRepository = userRepository;
        _lockoutOptions = lockoutOptions.Value;
    }

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(context.Email, context.TenantId, ct);

        if (user is not null && user.IsLockedOut())
            throw new AccountLockedException(user.LockoutEnd!.Value);

        await NextAsync(context, ct);
    }
}
