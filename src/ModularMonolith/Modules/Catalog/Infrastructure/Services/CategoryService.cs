using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Catalog.Application.DTOs;
using ModularMonolith.Modules.Catalog.Application.Services;
using ModularMonolith.Modules.Catalog.Domain.Entities;
using ModularMonolith.Modules.Catalog.Domain.Exceptions;
using ModularMonolith.Modules.Catalog.Domain.Repositories;

namespace ModularMonolith.Modules.Catalog.Infrastructure.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUser _currentUser;

    public CategoryService(ICategoryRepository categoryRepository, ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<CategoryDto> GetByIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct)
            ?? throw new CategoryNotFoundException(categoryId);

        return MapToDto(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var categories = await _categoryRepository.GetAllAsync(tenantId, ct);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var category = Category.Create(request.TenantId, request.Name, request.Description, request.ParentCategoryId);

        await _categoryRepository.AddAsync(category, ct);
        await _categoryRepository.SaveChangesAsync(ct);

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct)
            ?? throw new CategoryNotFoundException(categoryId);

        category.Update(request.Name, request.Description, request.ParentCategoryId);
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync(ct);

        return MapToDto(category);
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct)
            ?? throw new CategoryNotFoundException(categoryId);

        category.SoftDelete(_currentUser.UserId);
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync(ct);
    }

    private static CategoryDto MapToDto(Category c) => new(
        c.Id,
        c.TenantId,
        c.Name,
        c.Slug,
        c.Description,
        c.ParentCategoryId,
        c.IsActive,
        c.CreatedAt);
}
