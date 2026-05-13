using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;

namespace ModularMonolith.Modules.Auth.Presentation.Controllers;

/// <summary>Authentication — register, login, token refresh and revocation.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Register a new user and receive tokens.</summary>
    /// <response code="201">User created; access and refresh tokens returned.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="409">A user with that e-mail already exists.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TokenResponseDto>.Created(result));
    }

    /// <summary>Authenticate a user and receive tokens.</summary>
    /// <response code="200">Authentication successful; tokens returned.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="403">Tenant is inactive.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, ipAddress, ct);
        return Ok(ApiResponse<TokenResponseDto>.Ok(result));
    }

    /// <summary>Exchange a valid refresh token for a new token pair.</summary>
    /// <response code="200">New tokens returned.</response>
    /// <response code="401">Refresh token invalid or expired.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request, ipAddress, ct);
        return Ok(ApiResponse<TokenResponseDto>.Ok(result));
    }

    /// <summary>Revoke a refresh token, invalidating the session.</summary>
    /// <response code="200">Token revoked.</response>
    /// <response code="401">Token invalid or caller is not authenticated.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("revoke")]
    [Authorize]
    [EnableRateLimiting("api")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request, CancellationToken ct)
    {
        await _authService.RevokeTokenAsync(request, ct);
        return Ok(ApiResponse.NoContent("Token revoked successfully."));
    }
}
