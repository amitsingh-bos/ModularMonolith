using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Enums;

namespace ModularMonolith.Modules.Auth.Domain.Repositories;

public interface ITwoFactorTokenRepository
{
    Task<TwoFactorToken?> GetActiveAsync(Guid userId, TwoFactorMethod method, string purpose, CancellationToken ct = default);
    Task<TwoFactorToken?> GetByHashAsync(Guid userId, string codeHash, string purpose, CancellationToken ct = default);
    Task AddAsync(TwoFactorToken token, CancellationToken ct = default);
    void Update(TwoFactorToken token);
    Task SaveChangesAsync(CancellationToken ct = default);
}
