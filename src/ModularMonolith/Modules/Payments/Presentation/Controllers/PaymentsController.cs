using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.Modules.Payments.Application.DTOs;
using ModularMonolith.Modules.Payments.Application.Services;

namespace ModularMonolith.Modules.Payments.Presentation.Controllers;

/// <summary>Payment lifecycle management — process, complete, fail, and refund payments.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUser _currentUser;

    public PaymentsController(IPaymentService paymentService, ICurrentUser currentUser)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
    }

    /// <summary>Get a payment by ID.</summary>
    /// <param name="id">Payment GUID.</param>
    /// <response code="200">Payment found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>payments.payments.read</c> permission.</response>
    /// <response code="404">Payment not found.</response>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Payments.PaymentsRead)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var payment = await _paymentService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    /// <summary>List payments for the caller's tenant with optional filtering and paging.</summary>
    /// <response code="200">Paged payment list.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>payments.payments.read</c> permission.</response>
    [HttpGet]
    [RequirePermission(Permissions.Payments.PaymentsRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PaymentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPayments([FromQuery] GetPaymentsRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not available.");

        var result = await _paymentService.GetPaymentsAsync(tenantId, request, ct);
        return Ok(ApiResponse<IReadOnlyList<PaymentDto>>.OkPaged(result.Items, result.ToPaginationMeta()));
    }

    /// <summary>Get a payment by the associated order ID.</summary>
    /// <param name="orderId">Order GUID.</param>
    /// <response code="200">Payment found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>payments.payments.read</c> permission.</response>
    /// <response code="404">No payment found for the given order.</response>
    [HttpGet("order/{orderId:guid}")]
    [RequirePermission(Permissions.Payments.PaymentsRead)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken ct)
    {
        var payment = await _paymentService.GetByOrderIdAsync(orderId, ct);
        if (payment is null)
            return NotFound(ApiResponse.Fail($"No payment found for order '{orderId}'.", StatusCodes.Status404NotFound));

        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    /// <summary>Initiate a new payment for an order.</summary>
    /// <response code="201">Payment created and pending processing.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>payments.payments.write</c> permission.</response>
    [HttpPost]
    [RequirePermission(Permissions.Payments.PaymentsWrite)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        var payment = await _paymentService.ProcessAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PaymentDto>.Created(payment));
    }

    /// <summary>Mark a pending payment as completed.</summary>
    /// <param name="id">Payment GUID.</param>
    /// <param name="transactionReference">External gateway transaction reference.</param>
    /// <param name="gatewayResponse">Raw response payload from the payment gateway.</param>
    /// <response code="200">Payment completed.</response>
    /// <response code="400">Payment is not in Pending state.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>payments.payments.write</c> permission.</response>
    /// <response code="404">Payment not found.</response>
    [HttpPost("{id:guid}/complete")]
    [RequirePermission(Permissions.Payments.PaymentsWrite)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromQuery] string? transactionReference,
        [FromQuery] string? gatewayResponse,
        CancellationToken ct)
    {
        var payment = await _paymentService.CompleteAsync(id, transactionReference, gatewayResponse, ct);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    /// <summary>Mark a pending payment as failed.</summary>
    /// <param name="id">Payment GUID.</param>
    /// <param name="failureReason">Human-readable reason for the failure.</param>
    /// <param name="gatewayResponse">Raw response payload from the payment gateway.</param>
    /// <response code="200">Payment marked as failed.</response>
    /// <response code="400">Payment is not in Pending state.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>payments.payments.write</c> permission.</response>
    /// <response code="404">Payment not found.</response>
    [HttpPost("{id:guid}/fail")]
    [RequirePermission(Permissions.Payments.PaymentsWrite)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fail(
        Guid id,
        [FromQuery] string failureReason,
        [FromQuery] string? gatewayResponse,
        CancellationToken ct)
    {
        var payment = await _paymentService.FailAsync(id, failureReason, gatewayResponse, ct);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    /// <summary>Refund a completed payment.</summary>
    /// <param name="id">Payment GUID.</param>
    /// <response code="200">Payment refunded.</response>
    /// <response code="400">Payment is not in Completed state, or refund amount exceeds original amount.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>payments.payments.write</c> permission.</response>
    /// <response code="404">Payment not found.</response>
    [HttpPost("{id:guid}/refund")]
    [RequirePermission(Permissions.Payments.PaymentsWrite)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundPaymentRequest request, CancellationToken ct)
    {
        var payment = await _paymentService.RefundAsync(id, request, ct);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }
}
