using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.Modules.Auth.Application.Abstractions;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Infrastructure.Persistence;

/// <summary>
/// Seeds reference data on every startup (idempotent — skips rows that already exist).
/// Demo tenant: 00000000-0000-0000-0000-000000000001
/// Admin credentials: admin@demo.com / Admin@1234
/// </summary>
public sealed class DatabaseSeeder
{
    // Well-known demo identifiers — matches the DEMO_TENANT_ID constant in the React UI
    public static readonly Guid DemoTenantId = new("00000000-0000-0000-0000-000000000001");
    public const string AdminEmail = "admin@demo.com";
    public const string AdminPassword = "Admin@1234";

    private readonly AuthDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(AuthDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedPermissionsAsync(ct);
        await SeedDemoTenantAndAdminAsync(ct);
    }

    // ── Permissions ───────────────────────────────────────────────────────────

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existingCodes = await _context.Permissions
            .Select(p => p.Code)
            .ToListAsync(ct);

        var definitions = new[]
        {
            (Permissions.Auth.UsersRead,  "View users and their role assignments",          "Auth"),
            (Permissions.Auth.UsersWrite, "Assign and revoke roles on users",               "Auth"),
            (Permissions.Auth.RolesRead,  "View roles and their permission assignments",    "Auth"),
            (Permissions.Auth.RolesWrite, "Create, delete and configure roles",             "Auth"),
            (Permissions.Catalog.ProductsRead,    "View products",                          "Catalog"),
            (Permissions.Catalog.ProductsWrite,   "Create, update, delete products and adjust stock", "Catalog"),
            (Permissions.Catalog.CategoriesRead,  "View categories",                        "Catalog"),
            (Permissions.Catalog.CategoriesWrite, "Create, update and delete categories",   "Catalog"),
            (Permissions.Orders.OrdersRead,    "View orders and order items",                "Orders"),
            (Permissions.Orders.OrdersWrite,   "Create, confirm, ship, deliver and cancel orders", "Orders"),
            (Permissions.Payments.PaymentsRead,  "View payments and transaction history",   "Payments"),
            (Permissions.Payments.PaymentsWrite, "Process, complete, fail and refund payments", "Payments"),
        };

        var toAdd = definitions
            .Where(d => !existingCodes.Contains(d.Item1))
            .Select(d => Permission.Create(d.Item1, d.Item2, d.Item3))
            .ToList();

        if (toAdd.Count == 0) return;

        await _context.Permissions.AddRangeAsync(toAdd, ct);
        await _context.SaveChangesAsync(ct);
    }

    // ── Demo tenant + admin user ───────────────────────────────────────────────

    private async Task SeedDemoTenantAndAdminAsync(CancellationToken ct)
    {
        // 1. Tenant
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == DemoTenantId, ct);
        if (tenant is null)
        {
            tenant = Tenant.Create("Demo Tenant", "demo", DemoTenantId);
            await _context.Tenants.AddAsync(tenant, ct);
            await _context.SaveChangesAsync(ct);
        }

        // 2. Admin role with all permissions (skip if already exists)
        var adminRole = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.TenantId == DemoTenantId && r.Name == "Admin", ct);

        if (adminRole is null)
        {
            adminRole = Role.Create(DemoTenantId, "Admin", "Full access", isSystemRole: true);

            var allPermissions = await _context.Permissions
                .Where(p => Permissions.All.Contains(p.Code))
                .ToListAsync(ct);

            foreach (var perm in allPermissions)
                adminRole.AddPermission(perm.Id);

            await _context.Roles.AddAsync(adminRole, ct);
            await _context.SaveChangesAsync(ct);
        }

        // 3. Admin user (skip if already exists)
        var adminExists = await _context.Users
            .AnyAsync(u => u.TenantId == DemoTenantId && u.Email.Value == AdminEmail, ct);

        if (!adminExists)
        {
            var passwordHash = _passwordHasher.Hash(AdminPassword);
            var admin = User.Create(DemoTenantId, AdminEmail, passwordHash, "Admin", "User");
            admin.AssignRole(adminRole.Id);

            await _context.Users.AddAsync(admin, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
