using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.Modules.Catalog.Application.DTOs;
using ModularMonolith.Modules.Catalog.Application.Services;

namespace ModularMonolith.Modules.Catalog.Presentation.Controllers;

/// <summary>Category management — CRUD for product categories.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUser _currentUser;

    public CategoriesController(ICategoryService categoryService, ICurrentUser currentUser)
    {
        _categoryService = categoryService;
        _currentUser = currentUser;
    }

    /// <summary>Get a category by ID.</summary>
    /// <param name="id">Category GUID.</param>
    /// <response code="200">Category found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>catalog.categories.read</c> permission.</response>
    /// <response code="404">Category not found.</response>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Catalog.CategoriesRead)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    /// <summary>List all categories for the caller's tenant.</summary>
    /// <response code="200">Category list.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>catalog.categories.read</c> permission.</response>
    [HttpGet]
    [RequirePermission(Permissions.Catalog.CategoriesRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is not available.");

        var categories = await _categoryService.GetAllAsync(tenantId, ct);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(categories));
    }

    /// <summary>Create a new category.</summary>
    /// <response code="201">Category created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>catalog.categories.write</c> permission.</response>
    /// <response code="409">A category with that slug already exists in the tenant.</response>
    [HttpPost]
    [RequirePermission(Permissions.Catalog.CategoriesWrite)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CategoryDto>.Created(category));
    }

    /// <summary>Update an existing category.</summary>
    /// <param name="id">Category GUID.</param>
    /// <response code="200">Category updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>catalog.categories.write</c> permission.</response>
    /// <response code="404">Category not found.</response>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.Catalog.CategoriesWrite)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    /// <summary>Soft-delete a category.</summary>
    /// <param name="id">Category GUID.</param>
    /// <response code="200">Category deleted.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Requires <c>catalog.categories.write</c> permission.</response>
    /// <response code="404">Category not found.</response>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Catalog.CategoriesWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(id, ct);
        return Ok(ApiResponse.NoContent("Category deleted successfully."));
    }
}
