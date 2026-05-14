using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Enums;

namespace ModularMonolith.Modules.Auth.Presentation.Controllers;

/// <summary>Two-factor authentication — setup, confirmation, verification and status.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth/2fa")]
[Produces("application/json")]
public sealed class TwoFactorController : ControllerBase
{
    private readonly ITwoFactorService _twoFactorService;
    private readonly ICurrentUser _currentUser;

    public TwoFactorController(ITwoFactorService twoFactorService, ICurrentUser currentUser)
    {
        _twoFactorService = twoFactorService;
        _currentUser = currentUser;
    }

    /// <summary>Initiate 2FA setup. Returns a QR URI for Google Authenticator or sends an OTP for Email/SMS.</summary>
    /// <response code="200">Setup initiated.</response>
    /// <response code="400">Validation error or invalid method.</response>
    [HttpPost("setup")]
    [Authorize]
    [EnableRateLimiting("api")]
    [ProducesResponseType(typeof(ApiResponse<Setup2FaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Setup([FromBody] Enable2FaRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<TwoFactorMethod>(request.Method, ignoreCase: true, out var method))
            return BadRequest(ApiResponse.Fail($"Unknown method '{request.Method}'. Use: GoogleAuthenticator, Email, Sms."));

        var result = await _twoFactorService.SetupAsync(_currentUser.UserId!.Value, method, request.PhoneNumber, ct);
        return Ok(ApiResponse<Setup2FaResponseDto>.Ok(result));
    }

    /// <summary>Confirm 2FA setup with the code from the authenticator app or OTP message.</summary>
    /// <response code="200">2FA enabled.</response>
    /// <response code="401">Invalid or expired code.</response>
    [HttpPost("confirm")]
    [Authorize]
    [EnableRateLimiting("api")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Confirm([FromBody] Confirm2FaSetupRequest request, CancellationToken ct)
    {
        await _twoFactorService.ConfirmSetupAsync(_currentUser.UserId!.Value, request.Code, ct);
        return Ok(ApiResponse.NoContent("Two-factor authentication enabled."));
    }

    /// <summary>Disable 2FA. Requires a valid current code to prevent unauthorized disabling.</summary>
    /// <response code="200">2FA disabled.</response>
    /// <response code="401">Invalid code.</response>
    [HttpDelete]
    [Authorize]
    [EnableRateLimiting("api")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Disable([FromBody] Disable2FaRequest request, CancellationToken ct)
    {
        await _twoFactorService.DisableAsync(_currentUser.UserId!.Value, request.Code, ct);
        return Ok(ApiResponse.NoContent("Two-factor authentication disabled."));
    }

    /// <summary>Get the current user's 2FA status.</summary>
    /// <response code="200">Status returned.</response>
    [HttpGet("status")]
    [Authorize]
    [EnableRateLimiting("api")]
    [ProducesResponseType(typeof(ApiResponse<TwoFactorStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var result = await _twoFactorService.GetStatusAsync(_currentUser.UserId!.Value, ct);
        return Ok(ApiResponse<TwoFactorStatusDto>.Ok(result));
    }

    /// <summary>Complete login by submitting the 2FA code after a TwoFactorChallenge response from /auth/login.</summary>
    /// <response code="200">Tokens returned.</response>
    /// <response code="401">Invalid or expired token / code.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("verify")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verify([FromBody] VerifyLoginTwoFactorRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _twoFactorService.VerifyLoginAsync(request.TwoFactorToken, request.Code, ipAddress, ct);
        return Ok(ApiResponse<TokenResponseDto>.Ok(result));
    }
}
