using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using ModularMonolith.BuildingBlocks.Infrastructure.Services;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Application.Pipelines.Login;
using ModularMonolith.Modules.Auth.Application.Pipelines.TokenRefresh;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;
using ModularMonolith.Modules.Auth.Infrastructure.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Services;

namespace ModularMonolith.Modules.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<RefreshTokenOptions>()
            .BindConfiguration(RefreshTokenOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddDbContext<AuthDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Default"))
                   .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));

        services.AddHttpContextAccessor();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddOptionsWithValidateOnStart<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName);
        services.AddOptionsWithValidateOnStart<SmsOptions>()
            .BindConfiguration(SmsOptions.SectionName);

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITwoFactorTokenRepository, TwoFactorTokenRepository>();

        services.AddScoped<ValidateCredentialsHandler>();
        services.AddScoped<CheckAccountStatusHandler>();
        services.AddScoped<CheckTenantStatusHandler>();
        services.AddScoped<RecordLoginAuditHandler>();
        services.AddScoped<Check2FaHandler>();
        services.AddScoped<GenerateTokensHandler>();
        services.AddScoped<LoadRefreshTokenHandler>();
        services.AddScoped<CheckRevocationHandler>();
        services.AddScoped<RotateTokenHandler>();
        services.AddScoped<GenerateNewJwtHandler>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<DatabaseSeeder>();

        services.AddValidatorsFromAssembly(typeof(AuthModule).Assembly);

        return services;
    }
}
