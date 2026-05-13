using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using ModularMonolith.BuildingBlocks.Infrastructure.Services;
using ModularMonolith.Modules.Orders.Application.Services;
using ModularMonolith.Modules.Orders.Domain.Repositories;
using ModularMonolith.Modules.Orders.Infrastructure.Persistence;
using ModularMonolith.Modules.Orders.Infrastructure.Repositories;
using ModularMonolith.Modules.Orders.Infrastructure.Services;

namespace ModularMonolith.Modules.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // shared building-block services — TryAdd so other module registrations are not duplicated
        services.AddHttpContextAccessor();
        services.TryAddScoped<IAuditLogger, AuditLogger>();
        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.TryAddScoped<ITenantContext, TenantContext>();
        services.TryAddSingleton<SoftDeleteInterceptor>();

        services.AddDbContext<OrdersDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "orders"))
                   .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IOrderService, OrderService>();

        services.AddValidatorsFromAssembly(typeof(OrdersModule).Assembly,
            filter: descriptor => descriptor.ValidatorType.Namespace?
                .StartsWith("ModularMonolith.Modules.Orders") == true);

        return services;
    }
}
