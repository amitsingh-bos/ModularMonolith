using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Auth.Infrastructure.Repositories;

public sealed class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
{
    private readonly AuthDbContext _context;

    public TenantRepository(AuthDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct) =>
        await AddEntityAsync(tenant, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
