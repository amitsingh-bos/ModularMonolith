using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using ModularMonolith.BuildingBlocks.Infrastructure.Services;
using ModularMonolith.Modules.Payments.Application.Services;
using ModularMonolith.Modules.Payments.Domain.Repositories;
using ModularMonolith.Modules.Payments.Infrastructure.Persistence;
using ModularMonolith.Modules.Payments.Infrastructure.Repositories;
using ModularMonolith.Modules.Payments.Infrastructure.Services;

namespace ModularMonolith.Modules.Payments;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // shared building-block services — TryAdd so Auth/Catalog registrations are not duplicated
        services.AddHttpContextAccessor();
        services.TryAddScoped<IAuditLogger, AuditLogger>();
        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.TryAddScoped<ITenantContext, TenantContext>();
        services.TryAddSingleton<SoftDeleteInterceptor>();

        services.AddDbContext<PaymentsDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "payments"))
                   .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IPaymentService, PaymentService>();

        services.AddValidatorsFromAssembly(typeof(PaymentsModule).Assembly,
            filter: descriptor => descriptor.ValidatorType.Namespace?
                .StartsWith("ModularMonolith.Modules.Payments") == true);

        return services;
    }
}
