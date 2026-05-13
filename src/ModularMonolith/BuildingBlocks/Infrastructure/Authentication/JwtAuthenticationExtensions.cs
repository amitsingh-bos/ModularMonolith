using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.BuildingBlocks.Infrastructure.Options;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Authentication;

/// <summary>
/// Registers JWT Bearer authentication and the ASP.NET Core authorization services.
/// </summary>
/// <remarks>
/// Configuration is read from the <c>Jwt</c> section of <c>appsettings.json</c>
/// (bound via <see cref="JwtOptions"/>):
/// <code>
/// "Jwt": {
///   "SecretKey": "...",   // min 32 chars, HMAC-SHA256 signing key
///   "Issuer":    "...",   // must match the iss claim in tokens
///   "Audience":  "...",   // must match the aud claim in tokens
///   "ExpiryMinutes": 15  // short-lived access token
/// }
/// </code>
///
/// Token validation parameters applied:
/// <list type="bullet">
///   <item><b>ValidateIssuerSigningKey</b> — rejects tokens not signed with the configured key.</item>
///   <item><b>ValidateIssuer / ValidateAudience</b> — prevents token reuse across services.</item>
///   <item><b>ValidateLifetime</b> — enforces expiry.</item>
///   <item><b>ClockSkew = Zero</b> — no grace period; tokens expire exactly on time.</item>
///   <item><b>MapInboundClaims = false</b> — preserves short claim names (<c>sub</c>, <c>role</c>)
///         instead of mapping them to long WS-Federation URIs.</item>
///   <item><b>RoleClaimType = "role"</b> — makes <c>[Authorize(Roles = "Admin")]</c> work with
///         the <c>role</c> claim emitted by <see cref="ModularMonolith.Modules.Auth.Infrastructure.Services.JwtTokenService"/>.</item>
/// </list>
/// </remarks>
public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication and authorization services.
    /// Reads JWT settings from the <c>Jwt</c> configuration section.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();

        return services;
    }
}
