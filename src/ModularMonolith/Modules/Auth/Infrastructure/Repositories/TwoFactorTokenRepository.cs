using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Enums;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Auth.Infrastructure.Repositories;

public sealed class TwoFactorTokenRepository : ITwoFactorTokenRepository
{
    private readonly AuthDbContext _context;

    public TwoFactorTokenRepository(AuthDbContext context) => _context = context;

    public Task<TwoFactorToken?> GetActiveAsync(
        Guid userId, TwoFactorMethod method, string purpose, CancellationToken ct = default) =>
        _context.TwoFactorTokens
            .Where(t => t.UserId == userId
                     && t.Method == method
                     && t.Purpose == purpose
                     && !t.IsUsed
                     && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(TwoFactorToken token, CancellationToken ct = default) =>
        await _context.TwoFactorTokens.AddAsync(token, ct);

    public void Update(TwoFactorToken token) =>
        _context.TwoFactorTokens.Update(token);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
