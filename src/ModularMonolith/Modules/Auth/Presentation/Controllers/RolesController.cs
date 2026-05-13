using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;

namespace ModularMonolith.Modules.Auth.Presentation.Controllers;

/// <summary>Role management — lookup roles for the current tenant.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
[Authorize]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ICurrentUser _currentUser;

    public RolesController(IRoleService roleService, ICurrentUser currentUser)
    {
        _roleService = roleService;
        _currentUser = currentUser;
    }

    /// <summary>Get a role by ID.</summary>
    /// <param name="id">Role GUID.</param>
    /// <response code="200">Role found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Role not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var role = await _roleService.GetByIdAsync(id, ct);
        if (role is null)
            return NotFound(ApiResponse.Fail($"Role '{id}' not found.", StatusCodes.Status404NotFound));

        return Ok(ApiResponse<RoleDto>.Ok(role));
    }

    /// <summary>List all roles for the caller's tenant.</summary>
    /// <response code="200">Role list.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not available.");

        var roles = await _roleService.GetAllAsync(tenantId, ct);
        return Ok(ApiResponse<IReadOnlyList<RoleDto>>.Ok(roles));
    }
}
