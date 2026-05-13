using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;

namespace ModularMonolith.Modules.Auth.Presentation.Controllers;

/// <summary>Role management — CRUD and permission assignment for tenant roles.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
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
    /// <response code="403">Requires <c>auth.roles.read</c> permission.</response>
    /// <response code="404">Role not found.</response>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Auth.RolesRead)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
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
    /// <response code="403">Requires <c>auth.roles.read</c> permission.</response>
    [HttpGet]
    [RequirePermission(Permissions.Auth.RolesRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not available.");

        var roles = await _roleService.GetAllAsync(tenantId, ct);
        return Ok(ApiResponse<IReadOnlyList<RoleDto>>.Ok(roles));
    }

    /// <summary>Create a new custom role for the caller's tenant.</summary>
    /// <response code="201">Role created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>auth.roles.write</c> permission.</response>
    [HttpPost]
    [RequirePermission(Permissions.Auth.RolesWrite)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var role = await _roleService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RoleDto>.Created(role));
    }

    /// <summary>Delete a custom role.</summary>
    /// <param name="id">Role GUID.</param>
    /// <response code="200">Role deleted.</response>
    /// <response code="400">Cannot delete a system role.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>auth.roles.write</c> permission.</response>
    /// <response code="404">Role not found.</response>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Auth.RolesWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _roleService.DeleteAsync(id, ct);
        return Ok(ApiResponse.NoContent("Role deleted successfully."));
    }

    /// <summary>Assign a permission to a role.</summary>
    /// <param name="id">Role GUID.</param>
    /// <response code="200">Permission assigned.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>auth.roles.write</c> permission.</response>
    /// <response code="404">Role or permission not found.</response>
    [HttpPost("{id:guid}/permissions")]
    [RequirePermission(Permissions.Auth.RolesWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignPermission(Guid id, [FromBody] AssignPermissionRequest request, CancellationToken ct)
    {
        await _roleService.AssignPermissionAsync(id, request, ct);
        return Ok(ApiResponse.NoContent("Permission assigned successfully."));
    }

    /// <summary>Remove a permission from a role.</summary>
    /// <param name="id">Role GUID.</param>
    /// <param name="permissionId">Permission GUID.</param>
    /// <response code="200">Permission removed.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>auth.roles.write</c> permission.</response>
    /// <response code="404">Role not found.</response>
    [HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
    [RequirePermission(Permissions.Auth.RolesWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePermission(Guid id, Guid permissionId, CancellationToken ct)
    {
        await _roleService.RemovePermissionAsync(id, permissionId, ct);
        return Ok(ApiResponse.NoContent("Permission removed successfully."));
    }
}
