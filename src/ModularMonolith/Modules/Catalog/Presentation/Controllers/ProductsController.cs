using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Catalog.Application.DTOs;
using ModularMonolith.Modules.Catalog.Application.Services;

namespace ModularMonolith.Modules.Catalog.Presentation.Controllers;

/// <summary>Product catalog — CRUD and stock management.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICurrentUser _currentUser;

    public ProductsController(IProductService productService, ICurrentUser currentUser)
    {
        _productService = productService;
        _currentUser = currentUser;
    }

    /// <summary>Get a product by ID.</summary>
    /// <param name="id">Product GUID.</param>
    /// <response code="200">Product found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Product not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>List products for the caller's tenant with optional search and paging.</summary>
    /// <response code="200">Paged product list.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not available.");

        var result = await _productService.GetProductsAsync(tenantId, request, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductDto>>.OkPaged(result.Items, result.ToPaginationMeta()));
    }

    /// <summary>Create a new product.</summary>
    /// <response code="201">Product created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="409">A product with that SKU already exists in the tenant.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ProductDto>.Created(product));
    }

    /// <summary>Update an existing product.</summary>
    /// <param name="id">Product GUID.</param>
    /// <response code="200">Product updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Product not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>Soft-delete a product.</summary>
    /// <param name="id">Product GUID.</param>
    /// <response code="200">Product deleted.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Product not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _productService.DeleteAsync(id, ct);
        return Ok(ApiResponse.NoContent("Product deleted successfully."));
    }

    /// <summary>Adjust the stock quantity of a product.</summary>
    /// <param name="id">Product GUID.</param>
    /// <response code="200">Stock adjusted.</response>
    /// <response code="400">Delta would push stock below zero.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Product not found.</response>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        await _productService.AdjustStockAsync(id, request, ct);
        return Ok(ApiResponse.NoContent("Stock adjusted successfully."));
    }
}
