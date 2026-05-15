using Microsoft.Extensions.Options;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class ValidateCredentialsHandler : ChainHandlerBase<LoginContext>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AccountLockoutOptions _lockoutOptions;

    public ValidateCredentialsHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IOptions<AccountLockoutOptions> lockoutOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _lockoutOptions = lockoutOptions.Value;
    }

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(context.Email, context.TenantId, ct);

        if (user is null || !_passwordHasher.Verify(context.Password, user.PasswordHash))
        {
            if (user is not null)
            {
                user.RecordFailedLogin(
                    _lockoutOptions.MaxFailedAttempts,
                    TimeSpan.FromMinutes(_lockoutOptions.LockoutDurationMinutes));
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync(ct);
            }
            throw new InvalidCredentialsException();
        }

        context.User = user;
        await NextAsync(context, ct);
    }
}
