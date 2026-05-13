using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string name, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    void Delete(Role role);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
