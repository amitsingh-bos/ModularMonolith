using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;

namespace ModularMonolith.Modules.Auth.Presentation.Controllers;

/// <summary>Permissions catalogue — read-only list of all system permissions.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/permissions")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
        => _permissionService = permissionService;

    /// <summary>List all permissions defined in the system.</summary>
    /// <response code="200">Full permission list.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>auth.roles.read</c> permission.</response>
    [HttpGet]
    [RequirePermission(Permissions.Auth.RolesRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PermissionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var permissions = await _permissionService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<PermissionDto>>.Ok(permissions));
    }
}
