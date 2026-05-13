using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Auth.Infrastructure.Repositories;

public sealed class UserRepository : RepositoryBase<User>, IUserRepository, IAudit
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct) =>
        _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant() && u.TenantId == tenantId, ct);

    public Task<bool> ExistsByEmailAsync(string email, Guid tenantId, CancellationToken ct) =>
        _context.Users.AnyAsync(
            u => u.Email.Value == email.ToLowerInvariant() && u.TenantId == tenantId, ct);

    public Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct) =>
        _context.Users.CountAsync(u => u.TenantId == tenantId, ct);

    public async Task AddAsync(User user, CancellationToken ct) =>
        await AddEntityAsync(user, ct);

    public void Update(User user) => UpdateEntity(user);

    public void Delete(User user) => DeleteEntity(user);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
