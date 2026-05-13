using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.Modules.Orders.Application.DTOs;
using ModularMonolith.Modules.Orders.Application.Services;

namespace ModularMonolith.Modules.Orders.Presentation.Controllers;

/// <summary>Order management — lifecycle operations for tenant orders.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICurrentUser _currentUser;

    public OrdersController(IOrderService orderService, ICurrentUser currentUser)
    {
        _orderService = orderService;
        _currentUser = currentUser;
    }

    /// <summary>Get an order by ID.</summary>
    /// <param name="id">Order GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>orders.orders.read</c> permission.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Orders.OrdersRead)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    /// <summary>List orders for the caller's tenant with optional filtering and paging.</summary>
    /// <param name="request">Filter and pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Paged order list.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>orders.orders.read</c> permission.</response>
    [HttpGet]
    [RequirePermission(Permissions.Orders.OrdersRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not available.");

        var result = await _orderService.GetOrdersAsync(tenantId, request, ct);
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.OkPaged(result.Items, result.ToPaginationMeta()));
    }

    /// <summary>Create a new order.</summary>
    /// <param name="request">Order details including items and shipping address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Order created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>orders.orders.write</c> permission.</response>
    [HttpPost]
    [RequirePermission(Permissions.Orders.OrdersWrite)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<OrderDto>.Created(order));
    }

    /// <summary>Confirm a pending order.</summary>
    /// <param name="id">Order GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order confirmed.</response>
    /// <response code="400">Order is not in Pending status.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>orders.orders.write</c> permission.</response>
    /// <response code="404">Order not found.</response>
    [HttpPost("{id:guid}/confirm")]
    [RequirePermission(Permissions.Orders.OrdersWrite)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var order = await _orderService.ConfirmAsync(id, ct);
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    /// <summary>Cancel an order.</summary>
    /// <param name="id">Order GUID.</param>
    /// <param name="request">Optional cancellation reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order cancelled.</response>
    /// <response code="400">Order is in a status that cannot be cancelled (Shipped, Delivered, or Refunded).</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>orders.orders.write</c> permission.</response>
    /// <response code="404">Order not found.</response>
    [HttpPost("{id:guid}/cancel")]
    [RequirePermission(Permissions.Orders.OrdersWrite)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CancelAsync(id, request, ct);
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    /// <summary>Mark a confirmed order as shipped.</summary>
    /// <param name="id">Order GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order marked as shipped.</response>
    /// <response code="400">Order is not in Confirmed status.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>orders.orders.write</c> permission.</response>
    /// <response code="404">Order not found.</response>
    [HttpPost("{id:guid}/ship")]
    [RequirePermission(Permissions.Orders.OrdersWrite)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ship(Guid id, CancellationToken ct)
    {
        var order = await _orderService.ShipAsync(id, ct);
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    /// <summary>Mark a shipped order as delivered.</summary>
    /// <param name="id">Order GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Order marked as delivered.</response>
    /// <response code="400">Order is not in Shipped status.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>orders.orders.write</c> permission.</response>
    /// <response code="404">Order not found.</response>
    [HttpPost("{id:guid}/deliver")]
    [RequirePermission(Permissions.Orders.OrdersWrite)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        var order = await _orderService.DeliverAsync(id, ct);
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }
}
