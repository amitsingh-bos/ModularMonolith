namespace ModularMonolith.BuildingBlocks.Infrastructure.Authorization;

/// <summary>
/// All permission codes recognised by the authorization system.
/// Each code maps to a named ASP.NET Core policy and is embedded as a
/// <c>permission</c> claim in the JWT access token.
/// </summary>
/// <remarks>
/// Naming convention: <c>{module}.{resource}.{action}</c>
/// — <c>read</c>  allows GET endpoints for that resource.
/// — <c>write</c> allows POST / PUT / PATCH / DELETE endpoints.
///
/// Use <see cref="RequirePermissionAttribute"/> on controller actions:
/// <code>
/// [RequirePermission(Permissions.Catalog.ProductsWrite)]
/// public async Task&lt;IActionResult&gt; Create(...)
/// </code>
/// </remarks>
public static class Permissions
{
    public static class Auth
    {
        public const string UsersRead  = "auth.users.read";
        public const string UsersWrite = "auth.users.write";
        public const string RolesRead  = "auth.roles.read";
        public const string RolesWrite = "auth.roles.write";
    }

    public static class Catalog
    {
        public const string ProductsRead    = "catalog.products.read";
        public const string ProductsWrite   = "catalog.products.write";
        public const string CategoriesRead  = "catalog.categories.read";
        public const string CategoriesWrite = "catalog.categories.write";
    }

    public static class Orders
    {
        public const string OrdersRead  = "orders.orders.read";
        public const string OrdersWrite = "orders.orders.write";
    }

    public static class Payments
    {
        public const string PaymentsRead  = "payments.payments.read";
        public const string PaymentsWrite = "payments.payments.write";
    }

    /// <summary>Every permission code in the system — used to register policies and seed the database.</summary>
    public static IReadOnlyList<string> All =>
    [
        Auth.UsersRead, Auth.UsersWrite,
        Auth.RolesRead, Auth.RolesWrite,
        Catalog.ProductsRead,    Catalog.ProductsWrite,
        Catalog.CategoriesRead,  Catalog.CategoriesWrite,
        Orders.OrdersRead,       Orders.OrdersWrite,
        Payments.PaymentsRead,   Payments.PaymentsWrite
    ];

    /// <summary>Permissions granted to a regular (non-admin) user on first registration.</summary>
    public static IReadOnlyList<string> UserDefault =>
    [
        Auth.RolesRead,
        Catalog.ProductsRead,
        Catalog.CategoriesRead,
        Orders.OrdersRead,
        Payments.PaymentsRead
    ];
}
