using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Application.Pipelines.Login;

public sealed class ValidateCredentialsHandler : ChainHandlerBase<LoginContext>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ValidateCredentialsHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(context.Email, context.TenantId, ct);

        if (user is null || !_passwordHasher.Verify(context.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        context.User = user;
        await NextAsync(context, ct);
    }
}
