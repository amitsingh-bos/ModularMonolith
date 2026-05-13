using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    void Delete(User user);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
