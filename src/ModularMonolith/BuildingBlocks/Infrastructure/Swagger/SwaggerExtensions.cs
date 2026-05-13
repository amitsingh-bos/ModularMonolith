using System.Reflection;
using Microsoft.OpenApi;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Swagger;

/// <summary>
/// Registers Swashbuckle/OpenAPI services and configures the Swagger UI middleware.
/// </summary>
/// <remarks>
/// Two extension methods are provided so the service registration and middleware
/// wiring stay cleanly separated:
/// <list type="bullet">
///   <item><see cref="AddApiSwagger"/> — call on <see cref="IServiceCollection"/> during app building.</item>
///   <item><see cref="UseApiSwagger"/> — call on <see cref="WebApplication"/> after building.</item>
/// </list>
///
/// Security: every endpoint requires a Bearer JWT by default (global
/// <c>OpenApiSecurityRequirement</c>).  Use <c>[AllowAnonymous]</c> on endpoints
/// that should be accessible without a token; Swagger still shows the lock icon
/// but the server will not enforce it for those routes.
///
/// XML comments: the project must have <c>&lt;GenerateDocumentationFile&gt;true&lt;/GenerateDocumentationFile&gt;</c>
/// in the <c>.csproj</c> for summary/response descriptions to appear in the UI.
/// </remarks>
public static class SwaggerExtensions
{
    /// <summary>Adds Swagger document generation with JWT Bearer security.</summary>
    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ModularMonolith API",
                Version = "v1",
                Description = "REST API for the Modular Monolith — Auth and Catalog modules."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT access token obtained from POST /api/v1/auth/login"
            });

            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", doc),
                    []
                }
            });

            var xmlPath = Path.Combine(AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    /// <summary>
    /// Mounts the Swagger JSON endpoint and Swagger UI at the application root (<c>/</c>).
    /// </summary>
    public static WebApplication UseApiSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ModularMonolith API v1");
            c.RoutePrefix = string.Empty;
        });

        return app;
    }
}
