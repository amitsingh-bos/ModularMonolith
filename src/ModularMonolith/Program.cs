using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Authentication;
using ModularMonolith.BuildingBlocks.Infrastructure.Events;
using ModularMonolith.BuildingBlocks.Infrastructure.Filters;
using ModularMonolith.BuildingBlocks.Infrastructure.Middleware;
using ModularMonolith.BuildingBlocks.Infrastructure.RateLimiting;
using ModularMonolith.BuildingBlocks.Infrastructure.Observability;
using ModularMonolith.BuildingBlocks.Infrastructure.Swagger;
using ModularMonolith.Modules.Auth;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;
using ModularMonolith.Modules.Catalog;
using ModularMonolith.Modules.Catalog.Infrastructure.Persistence;
using ModularMonolith.Modules.Orders;
using ModularMonolith.Modules.Orders.Infrastructure.Persistence;
using ModularMonolith.Modules.Payments;
using ModularMonolith.Modules.Payments.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddAuthModule(builder.Configuration);
    builder.Services.AddCatalogModule(builder.Configuration);
    builder.Services.AddOrdersModule(builder.Configuration);
    builder.Services.AddPaymentsModule(builder.Configuration);

    builder.Services.AddObservability(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

    // Custom domain event pipeline — no third-party bus required.
    // DomainEventDispatcher resolves IDomainEventHandler<T> from DI at runtime.
    // AddDomainEventHandlers scans the assembly and registers every handler as Scoped.
    builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    builder.Services.AddDomainEventHandlers(typeof(Program).Assembly);

    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactApp", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });

    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddApiRateLimiting();
    builder.Services.AddApiSwagger();

    builder.Services.AddControllers(options =>
        options.Filters.Add<FluentValidationFilter>());

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

    var app = builder.Build();

    // Apply pending migrations and seed reference data at startup (idempotent)
    using (var scope = app.Services.CreateScope())
    {
        await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }

    app.MapPrometheusScrapingEndpoint("/metrics");
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseApiSwagger();
    app.UseHttpsRedirection();
    app.UseCors("ReactApp");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter(); // after auth so user identity is available for the "api" policy
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
