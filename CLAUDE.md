# CLAUDE.md — Architecture & Code Instructions

Follow every rule in this file when creating or modifying any code in this repository.

---

## Tech Stack

- **Runtime:** .NET 9 / C#
- **Framework:** ASP.NET Core 9
- **Database:** PostgreSQL via Npgsql + EF Core 9
- **Architecture:** Modular Monolith — single `.csproj`, multiple logical modules
- **Auth:** JWT (HMAC-SHA256) + Refresh tokens + BCrypt passwords
- **Validation:** FluentValidation (auto-discovered, auto-executed as action filter)
- **Logging:** Serilog with Correlation ID enrichment
- **Observability:** OpenTelemetry + Prometheus

---

## Project Layout

```
src/ModularMonolith/
├── BuildingBlocks/
│   ├── Domain/          # Entity, AggregateRoot, ValueObject, IDomainEvent, exceptions
│   ├── Application/     # ChainHandlerBase, ApiResponse, ICurrentUser, IDomainEventDispatcher
│   └── Infrastructure/  # BaseDbContext, RepositoryBase, middleware, options, JWT setup
├── Modules/
│   └── {Module}/
│       ├── Domain/           # Entities, ValueObjects, Events, Exceptions, Repository interfaces, Enums
│       ├── Application/      # DTOs, Service interfaces, Validators, Pipeline handlers
│       ├── Infrastructure/   # DbContext, EF configs, Repository impls, Services
│       └── Presentation/     # Controllers
├── Migrations/          # All EF migrations (centralized, not per-module)
└── appsettings.json
```

**Module boundaries are hard.** A module's code must not import from another module's namespace. Shared types live in `BuildingBlocks` only.

---

## Domain Layer Rules

**Location:** `Modules/{Module}/Domain/`

### Entities

- All entities inherit `AggregateRoot` (which inherits `Entity`).
- `Entity.Id` is `Guid`, set in the static factory, never elsewhere.
- Private parameterless constructor required for EF hydration.
- All properties have `private set` or `private init`. No public setters.
- State changes happen only through named domain methods.
- Use `RaiseDomainEvent(new SomeEvent(...))` inside domain methods — never from outside.

```csharp
public sealed class Order : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    private Order() { }

    public Guid TenantId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public static Order Create(Guid tenantId, ...)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, tenantId));
        return order;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel a shipped order.");
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, TenantId));
    }
}
```

### Value Objects

- Inherit `ValueObject`.
- Immutable — no setters.
- Static factory `Create()` for construction and validation.
- Owned by the aggregate's EF config via `OwnsOne()`.
- Override `GetEqualityComponents()` with all identifying fields.

```csharp
public sealed class Email : ValueObject
{
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Email Create(string email) => new(email.Trim().ToLowerInvariant());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Domain Events

- Use `record` (not class).
- Include `EventId = Guid.NewGuid()` and `OccurredAt = DateTime.UtcNow`.
- Name: `{Past-tense action}DomainEvent`.
- Include all context handlers will need — they cannot query the database.

```csharp
public sealed record OrderCreatedDomainEvent(Guid OrderId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

### Domain Exceptions

- Inherit `DomainException` (from `BuildingBlocks.Domain.Exceptions`).
- Message is hardcoded in constructor — no parameters to message.
- `sealed class` with no public properties unless the handler needs structured data (e.g., `AccountLockedException.LockoutEnd`).

```csharp
public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName)
        : base($"Insufficient stock for product '{productName}'.") { }
}
```

- After adding a new exception, map it in `ExceptionHandlingMiddleware` before the `DomainException` catch-all.

### Repository Interfaces

- Location: `Domain/Repositories/`
- Name: `I{Entity}Repository`
- Standard methods:

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    void Delete(Order order);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

- `Update` and `Delete` are synchronous (EF change tracking).
- `SaveChangesAsync` is always present — repositories own the save call.
- No EF types, no `DbSet`, no `IQueryable` in the interface.

---

## Application Layer Rules

**Location:** `Modules/{Module}/Application/`

### DTOs

- **Request DTOs:** `sealed record` with positional parameters or `init` properties.
- **Response DTOs:** `sealed class` or `sealed record` with `init` properties.
- Optional fields default to `null`.

```csharp
public sealed record CreateOrderRequest(
    Guid TenantId,
    Guid ProductId,
    int Quantity,
    string? Notes = null);

public sealed class OrderDto
{
    public Guid Id { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

### Validators

- One validator per DTO: `{DtoName}Validator : AbstractValidator<{Dto}>`.
- Location: `Application/Validators/`.
- No manual validation in controllers or services — the filter handles it.

```csharp
public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
```

### Service Interfaces

- Location: `Application/Services/`
- Name: `I{Concept}Service`
- Accept `CancellationToken` as last parameter.
- Return DTOs or `void/Task` — never domain entities, never `IActionResult`.

```csharp
public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task CancelAsync(Guid orderId, CancellationToken ct = default);
}
```

### Pipeline Handlers (Chain of Responsibility)

Use for complex workflows with multiple sequential steps (e.g., login, payment processing).

**Context object** holds mutable state passed through the chain:
```csharp
public sealed class LoginContext
{
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? IpAddress { get; init; }

    // Populated by handlers:
    public User? User { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public TokenResponseDto? Result { get; set; }
}
```

**Handler** inherits `ChainHandlerBase<TContext>`:
```csharp
public sealed class ValidateCredentialsHandler : ChainHandlerBase<LoginContext>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ValidateCredentialsHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public override async Task HandleAsync(LoginContext context, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(context.Email, context.TenantId, ct);
        if (user is null || !_passwordHasher.Verify(context.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        context.User = user;
        await NextAsync(context, ct); // pass to next handler
    }
}
```

**Wiring in service:**
```csharp
_checkAccountLockout
    .SetNext(_validateCredentials)
    .SetNext(_checkAccountStatus)
    .SetNext(_generateTokens);

await _checkAccountLockout.HandleAsync(context, ct);
return context.Result!;
```

- Each handler is registered as `Scoped` in DI individually.
- The service that wires them also receives them via constructor injection.
- Throwing from a handler short-circuits the chain.

### Domain Event Handlers

- Implement `IDomainEventHandler<TEvent>`.
- Location: `Application/EventHandlers/`.
- Auto-registered via `AddDomainEventHandlers()`.
- Dispatched **after** `SaveChangesAsync` commits.

```csharp
public sealed class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(OrderCreatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Order {OrderId} created", domainEvent.OrderId);
        return Task.CompletedTask;
    }
}
```

---

## Infrastructure Layer Rules

**Location:** `Modules/{Module}/Infrastructure/`

### DbContext

- Inherit `BaseDbContext`.
- Expose entities as `DbSet<T>` properties using expression-bodied `=>Set<T>()`.
- Apply configurations only from this module's namespace.

```csharp
public sealed class OrdersDbContext : BaseDbContext
{
    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        ICurrentUser currentUser,
        IDomainEventDispatcher dispatcher)
        : base(options, tenantContext, auditLogger, currentUser, dispatcher) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrdersDbContext).Assembly,
            t => t.Namespace?.StartsWith("ModularMonolith.Modules.Orders.Infrastructure.Persistence.Configurations") == true);

        base.OnModelCreating(modelBuilder);
    }
}
```

### EF Configurations

- Location: `Infrastructure/Persistence/Configurations/`
- One file per entity: `{Entity}Configuration.cs`
- Implement `IEntityTypeConfiguration<T>`

**Rules:**
- Table names: **snake_case** — `builder.ToTable("order_items")`
- Timestamps: always `timestamp with time zone` (EF Core default for PostgreSQL with `DateTime` using UTC)
- String columns: always specify `HasMaxLength()`
- Soft delete: always add `builder.HasQueryFilter(e => !e.IsDeleted)` for `ISoftDeletable` entities
- Value objects: use `OwnsOne()` and rename the flattened column with `HasColumnName()`
- Indexes: always index on `{TenantId, IsDeleted}` and any commonly queried combination

```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(o => o.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(o => new { o.TenantId, o.IsDeleted });
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
```

### Repository Implementations

- Inherit `RepositoryBase<TEntity>` and implement the domain interface.
- Add `IAudit` marker interface if changes should be audit-logged.
- Call protected base methods: `AddEntityAsync`, `UpdateEntity`, `DeleteEntity`.
- Eager load navigation properties in queries (no lazy loading).

```csharp
public sealed class OrderRepository : RepositoryBase<Order>, IOrderRepository, IAudit
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger) => _context = context;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct) =>
        await AddEntityAsync(order, ct);

    public void Update(Order order) => UpdateEntity(order);
    public void Delete(Order order) => DeleteEntity(order);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
```

### Migrations

- Always run migrations using `--context {ModuleDbContext} --configuration Release`.
- After generating a migration, **rebuild with Release** before running `database update`.
- Name migrations descriptively: `AddBruteForceProtection`, `AddProductCategories`.
- Migrations live in `src/ModularMonolith/Migrations/` (centralized).

---

## Presentation Layer Rules

**Location:** `Modules/{Module}/Presentation/Controllers/`

### Controller Structure

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpPost]
    [EnableRateLimiting("api")]
    [RequirePermission(Permissions.Orders.Write)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await _orderService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<OrderDto>.Created(result));
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("api")]
    [RequirePermission(Permissions.Orders.Read)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _orderService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }
}
```

### HTTP Method & Status Code Conventions

| Action | Method | Success Status |
|--------|--------|----------------|
| Create | POST | 201 Created |
| Get by ID | GET | 200 OK |
| List / Search | GET | 200 OK |
| Update | PUT | 200 OK |
| Delete | DELETE | 200 OK |
| Sub-resource action | POST | 200 OK |

### Response Wrapper

Every endpoint returns `ApiResponse<T>` or `ApiResponse`:
- Success: `ApiResponse<T>.Ok(data)`, `ApiResponse<T>.Created(data)`, `ApiResponse<T>.OkPaged(data, pagination)`
- No content: `ApiResponse.NoContent("message")`
- Errors are handled by `ExceptionHandlingMiddleware` — controllers never return error responses directly.

### Rate Limiting

- Public endpoints (login, register, refresh): `[EnableRateLimiting("auth")]` — 5 req/min per IP
- Authenticated endpoints: `[EnableRateLimiting("api")]` — 100 req/min per user

### Authorization

- Use `[RequirePermission(Permissions.{Module}.{Action})]` — not `[Authorize(Roles = "...")]`.
- Public endpoints: `[AllowAnonymous]`.
- Permissions are string constants defined in `BuildingBlocks.Infrastructure.Authorization.Permissions`.

### Pagination

For list endpoints, accept a paged query:
```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] GetOrdersRequest request, CancellationToken ct)
{
    var (items, pagination) = await _orderService.GetAllAsync(request, ct);
    return Ok(ApiResponse<IReadOnlyList<OrderDto>>.OkPaged(items, pagination));
}
```

---

## Exception Handling Rules

All exceptions bubble up to `ExceptionHandlingMiddleware`. Never catch exceptions in controllers.

**Mapping (order matters — specific before general):**

| Exception | HTTP Status |
|-----------|-------------|
| `ValidationException` | 400 |
| `InvalidCredentialsException` | 401 |
| `InvalidTokenException` | 401 |
| `AccountLockedException` | 423 |
| `UserAlreadyExistsException` | 409 |
| `TenantInactiveException` | 403 |
| `DomainException` (catch-all) | 400 |
| `NotFoundException` | 404 |
| `UnauthorizedAccessException` | 401 |
| Anything else | 500 |

**When adding a new domain exception:**
1. Create `{Name}Exception : DomainException` in `Domain/Exceptions/`.
2. Add a case in `ExceptionHandlingMiddleware` **before** the `DomainException` line.

---

## Dependency Injection Rules

### Module Registration

Each module registers via a static extension method in `{Module}Module.cs`:

```csharp
public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<SomeOptions>()
            .BindConfiguration(SomeOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddDbContext<OrdersDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Default"))
                   .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<SomePipelineHandler>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddValidatorsFromAssembly(typeof(OrdersModule).Assembly);

        return services;
    }
}
```

### Lifetimes

| Type | Lifetime |
|------|----------|
| DbContext | Scoped |
| Repository | Scoped |
| Application service | Scoped |
| Pipeline handler | Scoped |
| Event handler | Scoped |
| Validator | Scoped |
| Options | Singleton (framework-managed) |
| `SoftDeleteInterceptor` | Singleton |

---

## Options / Configuration Rules

### Options Class

```csharp
public sealed class AccountLockoutOptions
{
    public const string SectionName = "AccountLockout";

    [Range(1, 100)] public int MaxFailedAttempts { get; init; } = 5;
    [Range(1, 1440)] public int LockoutDurationMinutes { get; init; } = 15;
}
```

- `sealed class` with `public const string SectionName`.
- `init` properties with sensible defaults.
- Validation attributes on every property.
- Register with `AddOptionsWithValidateOnStart` + `ValidateDataAnnotations` so failures surface at startup.

### appsettings.json

Add a new section at the root level, matching `SectionName`:
```json
{
  "AccountLockout": {
    "MaxFailedAttempts": 5,
    "LockoutDurationMinutes": 15
  }
}
```

---

## Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Entity / Aggregate | `{Noun}` | `Order`, `Product` |
| Domain event | `{PastTenseAction}DomainEvent` | `OrderCreatedDomainEvent` |
| Domain exception | `{Condition}Exception` | `InsufficientStockException` |
| Value object | `{Concept}` | `Money`, `Email` |
| Repository interface | `I{Entity}Repository` | `IOrderRepository` |
| Repository impl | `{Entity}Repository` | `OrderRepository` |
| Service interface | `I{Concept}Service` | `IOrderService` |
| Service impl | `{Concept}Service` | `OrderService` |
| Request DTO | `{Action}{Entity}Request` | `CreateOrderRequest` |
| Response DTO | `{Entity}Dto` | `OrderDto` |
| Validator | `{DtoName}Validator` | `CreateOrderRequestValidator` |
| Pipeline handler | `{Action}Handler` | `ValidateCredentialsHandler` |
| Event handler | `{EventName}Handler` | `OrderCreatedEventHandler` |
| Controller | `{Resource}Controller` | `OrdersController` |
| DbContext | `{Module}DbContext` | `OrdersDbContext` |
| EF config | `{Entity}Configuration` | `OrderConfiguration` |
| Migration | `{Description}` | `AddBruteForceProtection` |
| Options class | `{Feature}Options` | `AccountLockoutOptions` |

### Namespaces

```
ModularMonolith.Modules.{Module}.Domain.Entities
ModularMonolith.Modules.{Module}.Domain.Events
ModularMonolith.Modules.{Module}.Domain.Exceptions
ModularMonolith.Modules.{Module}.Domain.Repositories
ModularMonolith.Modules.{Module}.Application.DTOs
ModularMonolith.Modules.{Module}.Application.Services
ModularMonolith.Modules.{Module}.Application.Validators
ModularMonolith.Modules.{Module}.Application.Pipelines.{Feature}
ModularMonolith.Modules.{Module}.Application.EventHandlers
ModularMonolith.Modules.{Module}.Infrastructure.Persistence
ModularMonolith.Modules.{Module}.Infrastructure.Persistence.Configurations
ModularMonolith.Modules.{Module}.Infrastructure.Repositories
ModularMonolith.Modules.{Module}.Infrastructure.Services
ModularMonolith.Modules.{Module}.Presentation.Controllers
```

---

## Permissions

Add new permissions as string constants in `BuildingBlocks.Infrastructure.Authorization.Permissions`:

```csharp
public static class Permissions
{
    public static class Orders
    {
        public const string Read  = "orders.read";
        public const string Write = "orders.write";
    }

    public static IReadOnlyList<string> All => [
        // include every permission from every module
    ];
}
```

Format: `{module}.{resource}.{action}` — all lowercase, dots as separators.

---

## Key Rules Summary

1. **No public setters on entities.** All state changes through named methods.
2. **Static `Create()` factory on every aggregate.** Never `new` an aggregate from outside the domain.
3. **Raise domain events inside domain methods.** Never from services or handlers.
4. **No DbContext in Application layer.** Services and handlers inject repository interfaces only.
5. **No business logic in controllers.** Controllers call services and return `ApiResponse<T>`.
6. **Throw domain exceptions; never return null for errors.** `NotFoundException` for missing entities.
7. **All new exceptions must be mapped in `ExceptionHandlingMiddleware`.**
8. **Every DTO needs a FluentValidation validator.** No `[Required]` attributes on DTOs.
9. **All options classes use `AddOptionsWithValidateOnStart` + `ValidateDataAnnotations`.**
10. **After any new entity field, update EF config, create a migration, rebuild Release, apply.**
11. **Pipeline handlers are individually Scoped in DI** and wired by the owning service.
12. **One module must not import from another module's namespace.** Shared types go in `BuildingBlocks`.
13. **All timestamps are `DateTime.UtcNow`.** Never `DateTime.Now`.
14. **Table names are snake_case.** Always set explicitly with `builder.ToTable("name")`.
15. **Soft-deletable entities always have `HasQueryFilter(e => !e.IsDeleted)` in EF config.**
