using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Auth.Infrastructure.Repositories;

public sealed class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    private readonly AuthDbContext _context;

    public RoleRepository(AuthDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Role?> GetByNameAsync(string name, Guid tenantId, CancellationToken ct) =>
        _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name && r.TenantId == tenantId, ct);

    public Task<IReadOnlyList<Role>> GetAllAsync(Guid tenantId, CancellationToken ct) =>
        _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Role>)t.Result, ct);

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct) =>
        _context.Roles.AnyAsync(r => r.Id == id, ct);

    public async Task AddAsync(Role role, CancellationToken ct) =>
        await AddEntityAsync(role, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
