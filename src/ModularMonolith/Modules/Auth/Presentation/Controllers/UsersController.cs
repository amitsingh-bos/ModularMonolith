using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;

namespace ModularMonolith.Modules.Auth.Presentation.Controllers;

/// <summary>User management — lookup and role assignment.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;

    public UsersController(IUserService userService, ICurrentUser currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    /// <summary>Get a user by ID.</summary>
    /// <param name="id">User GUID.</param>
    /// <response code="200">User found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>List users for the caller's tenant with optional search and paging.</summary>
    /// <response code="200">Paged user list.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not available.");

        var result = await _userService.GetUsersAsync(tenantId, request, ct);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.OkPaged(result.Items, result.ToPaginationMeta()));
    }

    /// <summary>Assign a role to a user.</summary>
    /// <param name="id">User GUID (must match <c>request.UserId</c>).</param>
    /// <response code="200">Role assigned.</response>
    /// <response code="400">ID mismatch or validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User or role not found.</response>
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        if (id != request.UserId)
            return BadRequest(ApiResponse.Fail("User ID in path must match request body."));

        await _userService.AssignRoleAsync(request, ct);
        return Ok(ApiResponse.NoContent("Role assigned successfully."));
    }
}
