using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Auth.Infrastructure.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AuthDbContext _context;

    public PermissionRepository(AuthDbContext context) => _context = context;

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct) =>
        await _context.Permissions.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct)
    {
        var codeList = codes.ToList();
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => codeList.Contains(p.Code))
            .ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
        _context.Permissions.AnyAsync(p => p.Id == id, ct);
}
