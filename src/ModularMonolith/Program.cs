using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Infrastructure.Authentication;
using ModularMonolith.BuildingBlocks.Infrastructure.Filters;
using ModularMonolith.BuildingBlocks.Infrastructure.Middleware;
using ModularMonolith.BuildingBlocks.Infrastructure.RateLimiting;
using ModularMonolith.BuildingBlocks.Infrastructure.Swagger;
using ModularMonolith.Modules.Auth;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;
using ModularMonolith.Modules.Catalog;
using ModularMonolith.Modules.Catalog.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    builder.Services.AddAuthModule(builder.Configuration);
    builder.Services.AddCatalogModule(builder.Configuration);

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

    // Apply pending migrations at startup (safe to run multiple times)
    using (var scope = app.Services.CreateScope())
    {
        await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    }

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseApiSwagger();
    app.UseHttpsRedirection();
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
