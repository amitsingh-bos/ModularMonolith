using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext : BaseDbContext
{
    public AuthDbContext(
        DbContextOptions<AuthDbContext> options,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        ICurrentUser currentUser,
        IDomainEventDispatcher dispatcher)
        : base(options, tenantContext, auditLogger, currentUser, dispatcher) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly,
            t => t.Namespace?.StartsWith("ModularMonolith.Modules.Auth.Infrastructure.Persistence.Configurations") == true);

        base.OnModelCreating(modelBuilder);
    }
}
