using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    void Update(RefreshToken token);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
