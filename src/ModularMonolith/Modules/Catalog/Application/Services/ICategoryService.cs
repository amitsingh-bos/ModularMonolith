using ModularMonolith.Modules.Catalog.Application.DTOs;

namespace ModularMonolith.Modules.Catalog.Application.Services;

public interface ICategoryService
{
    Task<CategoryDto> GetByIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid categoryId, CancellationToken ct = default);
}
