using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using ModularMonolith.BuildingBlocks.Infrastructure.Services;
using ModularMonolith.Modules.Catalog.Application.Services;
using ModularMonolith.Modules.Catalog.Domain.Repositories;
using ModularMonolith.Modules.Catalog.Infrastructure.Persistence;
using ModularMonolith.Modules.Catalog.Infrastructure.Repositories;
using ModularMonolith.Modules.Catalog.Infrastructure.Services;

namespace ModularMonolith.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        // shared building-block services — TryAdd so Auth registrations are not duplicated
        services.AddHttpContextAccessor();
        services.TryAddScoped<IAuditLogger, AuditLogger>();
        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.TryAddScoped<ITenantContext, TenantContext>();
        services.TryAddSingleton<SoftDeleteInterceptor>();

        services.AddDbContext<CatalogDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"))
                   .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddValidatorsFromAssembly(typeof(CatalogModule).Assembly,
            filter: descriptor => descriptor.ValidatorType.Namespace?
                .StartsWith("ModularMonolith.Modules.Catalog") == true);

        return services;
    }
}
