using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Auth.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository, IAudit
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct) =>
        _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct) =>
        await AddEntityAsync(token, ct);

    public void Update(RefreshToken token) => UpdateEntity(token);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.Revoke();
            UpdateEntity(token);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
