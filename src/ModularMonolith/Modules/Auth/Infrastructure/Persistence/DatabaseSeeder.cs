using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Infrastructure.Authorization;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.Modules.Auth.Infrastructure.Persistence;

/// <summary>
/// Seeds the <c>permissions</c> table with every known permission code on startup.
/// Safe to run on every restart — skips codes that already exist.
/// </summary>
public sealed class DatabaseSeeder
{
    private readonly AuthDbContext _context;

    public DatabaseSeeder(AuthDbContext context) => _context = context;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedPermissionsAsync(ct);
    }

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
}
