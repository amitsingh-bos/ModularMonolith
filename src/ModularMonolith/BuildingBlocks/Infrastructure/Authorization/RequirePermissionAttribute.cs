using Microsoft.AspNetCore.Authorization;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Authorization;

/// <summary>
/// Requires the authenticated caller to hold the specified <c>permission</c> claim in their JWT.
/// </summary>
/// <remarks>
/// This is syntactic sugar over <c>[Authorize(Policy = "...")]</c>.
/// Every code in <see cref="Permissions"/> is pre-registered as an ASP.NET Core named policy
/// inside <see cref="ModularMonolith.BuildingBlocks.Infrastructure.Authentication.JwtAuthenticationExtensions"/>.
///
/// Usage:
/// <code>
/// [RequirePermission(Permissions.Catalog.ProductsWrite)]
/// public async Task&lt;IActionResult&gt; Create(...)
///
/// [RequirePermission(Permissions.Auth.RolesRead)]
/// public async Task&lt;IActionResult&gt; GetAll(...)
/// </code>
/// </remarks>
/// <param name="permission">A permission code from <see cref="Permissions"/>.</param>
public sealed class RequirePermissionAttribute(string permission) : AuthorizeAttribute(permission);
