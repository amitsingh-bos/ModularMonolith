# Build Log — Prompts Used to Create This Project

A sequential record of every prompt used to build this Modular Monolith boilerplate
from scratch with Claude Code. Replay them in order to recreate the entire project.

---

## Phase 1 — Project Scaffold & Solution Structure

```
Create a .NET 9 Modular Monolith solution with the following structure:

Solution: ModularMonolith
  src/
    ModularMonolith/          ← single host project
      BuildingBlocks/
        Domain/               (AggregateRoot, Entity, ValueObject, IDomainEvent, domain exceptions)
        Application/          (ApiResponse envelope, PagedResult, ICurrentUser, ITenantContext, IAuditLogger)
        Infrastructure/       (Middleware, Filters, Services, Options, Persistence, Multitenancy)
      Modules/
        Auth/                 (Domain, Application, Infrastructure, Presentation)
  tests/
    ModularMonolith.UnitTests
    ModularMonolith.IntegrationTests

Use:
- .NET 9 / ASP.NET Core
- EF Core 9 + Npgsql (PostgreSQL)
- FluentValidation
- Serilog
- Asp.Versioning.Mvc 8.x
- BCrypt.Net-Next for password hashing
- Microsoft.AspNetCore.Authentication.JwtBearer 9.x
```

---

## Phase 2 — Domain Building Blocks

```
Implement the DDD building blocks inside BuildingBlocks/Domain:

1. Entity base class
   - Guid Id (protected init so derived factories can set it)

2. AggregateRoot : Entity
   - Collects IDomainEvent
   - AddDomainEvent, ClearDomainEvents, GetDomainEvents

3. ValueObject
   - Structural equality via GetEqualityComponents()

4. IDomainEvent interface
   - Guid EventId
   - DateTime OccurredAt

5. Domain exceptions:
   - DomainException (base)
   - NotFoundException
   - InvalidCredentialsException
   - InvalidTokenException
   - UserAlreadyExistsException(string email)
   - TenantInactiveException
   - InsufficientStockException
```

---

## Phase 3 — Application Building Blocks

```
Implement shared application abstractions inside BuildingBlocks/Application:

1. ICurrentUser
   - Guid? UserId, Guid? TenantId
   - IReadOnlyList<string> Roles, Permissions

2. ITenantContext
   - Guid? TenantId

3. IAuditLogger
   - LogAsync(tableName, action, entityId, oldValues, newValues)

4. ApiResponse envelope (non-generic and generic)
   - success, statusCode, message, data, pagination
   - Static factories: Ok, Created, NoContent, Fail, ValidationError

5. PagedResult<T> with ToPaginationMeta()

6. IChainHandler<TContext> and ChainHandlerBase<TContext>
   - Chain of Responsibility pipeline pattern
   - SetNext(handler), HandleAsync(context, ct)
```

---

## Phase 4 — Infrastructure Building Blocks

```
Implement shared infrastructure inside BuildingBlocks/Infrastructure:

1. CurrentUser : ICurrentUser
   - Reads sub, tenant_id, role, permission claims from IHttpContextAccessor
   - claim names are short (MapInboundClaims = false)

2. TenantContext : ITenantContext
   - Reads tenant_id from IHttpContextAccessor

3. AuditLogger : IAuditLogger
   - Persists to audit_logs table via a DbContext

4. SoftDeleteInterceptor : SaveChangesInterceptor
   - On save: sets IsDeleted=true, DeletedAt, DeletedBy instead of deleting

5. ExceptionHandlingMiddleware
   - Maps exceptions to HTTP status codes:
     ValidationException       → 400
     InvalidCredentialsException → 401
     InvalidTokenException      → 401
     UserAlreadyExistsException → 409
     TenantInactiveException    → 403
     DomainException            → 400
     NotFoundException          → 404
     UnauthorizedAccessException → 401
     unhandled                  → 500
   - IMPORTANT: specific subclasses must appear before base DomainException in the switch

6. FluentValidationFilter : IActionFilter
   - Resolves IValidator<T> from DI, validates before controller executes
   - Returns ApiResponse.ValidationError on failure

7. JwtOptions (IOptions pattern, validated on start)
   - SecretKey, Issuer, Audience, ExpiryMinutes

8. RefreshTokenOptions
   - ExpiryDays
```

---

## Phase 5 — Auth Module

```
Build a complete Auth module at Modules/Auth with these layers:

DOMAIN
  Entities:
    - Tenant (Id, Name, Slug, IsActive, CreatedAt)
    - User : AggregateRoot (Id, TenantId, Email, PasswordHash, FirstName, LastName,
             IsActive, IsEmailVerified, LastLoginAt, soft-delete fields, audit fields)
             Methods: AssignRole(roleId), RemoveRole(roleId)
    - Role (Id, TenantId, Name, Description, IsSystemRole)
    - Permission (Id, Code, Description, Module)
    - UserRole (UserId, RoleId, AssignedAt)
    - RolePermission (RoleId, PermissionId)
    - RefreshToken (Id, UserId, TokenHash, ExpiresAt, IsRevoked, RevokedAt,
                    ReplacedByToken, DeviceInfo, IpAddress, CreatedAt)
                    Methods: Revoke(replacedBy), IsActive computed property

  Interfaces:
    - IUserRepository (GetByIdAsync, GetByEmailAsync, ExistsByEmailAsync, AddAsync, UpdateAsync)
    - IRoleRepository (GetByIdAsync, GetAllByTenantAsync)
    - IRefreshTokenRepository (GetByHashAsync, AddAsync, UpdateAsync)
    - IPasswordHasher (Hash, Verify)
    - ITokenService (GenerateAccessToken, GenerateRefreshToken, HashToken)

APPLICATION
  DTOs: RegisterRequest, LoginRequest, RefreshTokenRequest, RevokeTokenRequest,
        TokenResponseDto, UserDto, RoleDto, AssignRoleRequest, GetUsersRequest

  Pipeline contexts:
    - LoginContext (request, ipAddress → user, tenant, tokenResponse)
    - RefreshContext (request, ipAddress → refreshToken, user, tokenResponse)

  Services interfaces:
    - IAuthService (RegisterAsync, LoginAsync, RefreshTokenAsync, RevokeTokenAsync)
    - IUserService (GetByIdAsync, GetUsersAsync, AssignRoleAsync)
    - IRoleService (GetByIdAsync, GetAllAsync)

INFRASTRUCTURE
  Persistence:
    - AuthDbContext with tables: users, roles, permissions, tenants,
      user_roles, role_permissions, refresh_tokens, audit_logs
    - Table audit_logs CHECK constraint: "Action" IN ('Created','Updated','Deleted')
      (use quoted column name in the CHECK — PostgreSQL is case-sensitive)
    - Unique indexes: users(email), roles(TenantId,Name), permissions(Code),
      refresh_tokens(TokenHash)

  Services:
    - BCryptPasswordHasher : IPasswordHasher
    - JwtTokenService : ITokenService
      Claims: sub=userId, tenant_id, role (one per role), permission (one per permission)
      Refresh token: 64 random bytes → Base64
      HashToken: SHA256 → lowercase hex

    - AuthService : IAuthService
      RegisterAsync: check ExistsByEmail → hash password → create User →
                     save → create RefreshToken → generate tokens
      LoginAsync: chain ValidateCredentials→CheckAccountStatus→
                  CheckTenantStatus→RecordLoginAudit→GenerateTokens
      RefreshTokenAsync: chain LoadRefreshToken→CheckRevocation→
                         RotateToken→GenerateNewJwt
      RevokeTokenAsync: hash lookup → check IsActive → Revoke() → save

    - UserService : IUserService
    - RoleService : IRoleService

PRESENTATION
  Controllers (all [ApiVersion("1.0")]):
    - AuthController: POST register, login, refresh, revoke
    - UsersController: GET /{id}, GET / (paged), POST /{id}/roles
    - RolesController: GET /{id}, GET /
```

---

## Phase 6 — Catalog Module

```
Build a complete Catalog module at Modules/Catalog following the exact same
layered pattern as the Auth module.

DOMAIN
  Entities:
    - Category : AggregateRoot
      (Id, TenantId, Name, Slug, Description, IsActive, soft-delete, audit fields)
    - Product : AggregateRoot
      (Id, TenantId, CategoryId, Name, Description, Sku, Price,
       StockQuantity, IsActive, soft-delete, audit fields)
      - Category navigation property
      - SKU normalized to uppercase in constructor
      - AdjustStock(int delta): throws InsufficientStockException if result < 0

  Domain Events: ProductCreated, ProductUpdated, StockAdjusted (all implement IDomainEvent)

  Repositories: IProductRepository, ICategoryRepository

APPLICATION
  DTOs: ProductDto, CategoryDto, CreateProductRequest, UpdateProductRequest,
        AdjustStockRequest, GetProductsRequest, CreateCategoryRequest, UpdateCategoryRequest

  Services: IProductService, ICategoryService

INFRASTRUCTURE
  CatalogDbContext
    - Products table in "catalog" schema
    - Categories table in "catalog" schema
    - Unique index (Sku, TenantId) on Products
    - Unique index (Slug, TenantId) on Categories
    - FK Product→Category: OnDelete Restrict
    - Global query filters for soft-delete on both entities
    - MigrationsHistoryTable("__EFMigrationsHistory", "catalog")

  ProductRepository, CategoryRepository
  ProductService, CategoryService
    - After Create/Update reload entity to populate Category navigation property

PRESENTATION
  - ProductsController: GET /{id}, GET / (paged), POST, PUT /{id},
                        DELETE /{id}, PATCH /{id}/stock
  - CategoriesController: GET /{id}, GET /, POST, PUT /{id}, DELETE /{id}

MODULE REGISTRATION (CatalogModule.cs)
  Use TryAddScoped / TryAddSingleton for shared BuildingBlocks services
  (ICurrentUser, ITenantContext, IAuditLogger, SoftDeleteInterceptor)
  to avoid duplicate registrations with AuthModule.
```

---

## Phase 7 — Program.cs & App Wiring

```
Wire everything together in Program.cs:

- builder.Services.AddAuthModule(configuration)
- builder.Services.AddCatalogModule(configuration)
- JWT Bearer authentication (MapInboundClaims=false, ClockSkew=Zero, RoleClaimType="role")
- builder.Services.AddAuthorization()
- builder.Services.AddControllers with FluentValidationFilter
- API versioning (default v1, URL segment, SubstituteApiVersionInUrl=true)
- Serilog (bootstrap logger + host UseSerilog)

Middleware pipeline order:
  UseSerilogRequestLogging
  UseMiddleware<ExceptionHandlingMiddleware>
  UseSwagger / UseSwaggerUI (at root /)
  UseHttpsRedirection
  UseAuthentication
  UseAuthorization
  MapControllers

Connection string key: "Default"
  "Host=localhost;Port=5433;Database=db_ModularMonolith;Username=postgres;Password=postgres"

appsettings.json sections:
  Jwt: SecretKey, Issuer, Audience, ExpiryMinutes=15
  RefreshToken: ExpiryDays=7
```

---

## Phase 8 — Swagger Documentation

```
Implement full Swagger documentation across all controllers:

1. Configure Swashbuckle (Swashbuckle.AspNetCore 10.1.7) with Microsoft.OpenApi 2.x:
   - All types are in Microsoft.OpenApi root namespace (not Microsoft.OpenApi.Models)
   - Security definition: Bearer (HTTP, bearer, JWT)
   - Security requirement uses OpenApiSecuritySchemeReference and factory overload:
     c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement {
       { new OpenApiSecuritySchemeReference("Bearer", doc), [] }
     })
   - Include XML comments (GenerateDocumentationFile=true, NoWarn 1591;1573)

2. Add to every controller:
   - [Produces("application/json")]
   - XML <summary> on the class
   - XML <summary> + <response> tags on every action
   - [ProducesResponseType(typeof(ApiResponse<T>), StatusCode)] on every action
   - [AllowAnonymous] explicitly on anonymous endpoints
```

---

## Phase 9 — Rate Limiting

```
Add rate limiting using the built-in .NET 7+ Microsoft.AspNetCore.RateLimiting
(no extra NuGet package needed).

Create BuildingBlocks/Infrastructure/RateLimiting/RateLimiterExtensions.cs
with AddApiRateLimiting() extension method containing two policies:

"auth" — Fixed Window
  - PermitLimit: 5, Window: 60s, QueueLimit: 0
  - Partition key: remote IP address
  - Apply to: [EnableRateLimiting("auth")] on POST register, login, refresh

"api" — Sliding Window
  - PermitLimit: 100, Window: 60s, SegmentsPerWindow: 6, QueueLimit: 0
  - Partition key: JWT "sub" claim (fallback to IP if anonymous)
  - Apply to: [EnableRateLimiting("api")] on all other controllers (class level)

On rejection:
  - Status 429
  - Retry-After header
  - JSON body: {"success":false,"message":"Too many requests...","statusCode":429}

Middleware placement: app.UseRateLimiter() AFTER UseAuthorization
(so user identity is resolved before the "api" policy partition runs)

Add XML <remarks> to the class describing Fixed Window vs Sliding Window
with ASCII timeline diagrams and examples.
```

---

## Phase 10 — Extension File Refactoring

```
Split Program.cs infrastructure concerns into separate extension files:

1. BuildingBlocks/Infrastructure/Swagger/SwaggerExtensions.cs
   - AddApiSwagger(this IServiceCollection) → registers Swashbuckle + security
   - UseApiSwagger(this WebApplication) → mounts UI at /
   - XML <remarks> explaining global security requirement and XML docs setup

2. BuildingBlocks/Infrastructure/Authentication/JwtAuthenticationExtensions.cs
   - AddJwtAuthentication(this IServiceCollection, IConfiguration)
   - Reads Jwt section, configures AddAuthentication + AddJwtBearer + AddAuthorization
   - XML <remarks> listing all TokenValidationParameters and why each is set

Program.cs should end up calling only:
   builder.Services.AddAuthModule(builder.Configuration)
   builder.Services.AddCatalogModule(builder.Configuration)
   builder.Services.AddJwtAuthentication(builder.Configuration)
   builder.Services.AddApiRateLimiting()
   builder.Services.AddApiSwagger()
```

---

## Phase 11 — Docker

```
Add Docker support:

1. Dockerfile (multi-stage, at solution root)
   - Build stage: mcr.microsoft.com/dotnet/sdk:9.0
   - Runtime stage: mcr.microsoft.com/dotnet/aspnet:9.0
   - EXPOSE 8080

2. docker-compose.yml
   db:
     image: postgres:16-alpine
     healthcheck: pg_isready -U postgres -d db_ModularMonolith
     ports: "5433:5432"
     volume: postgres_data
   api:
     build: .
     ports: "8080:8080"
     environment:
       ASPNETCORE_ENVIRONMENT=Development
       ConnectionStrings__Default=Host=db;Port=5432;Database=db_ModularMonolith;...
     depends_on: db (condition: service_healthy)

3. .dockerignore
   Exclude: bin, obj, .vs, .vscode, .git, *.user, docker-compose.yml,
            Dockerfile, .template.config

4. Program.cs — apply migrations at startup:
   using (var scope = app.Services.CreateScope())
   {
       await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
       await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
   }

NOTE: Auth migration has a CHECK constraint bug — generated SQL uses lowercase
"action" but the column is quoted "Action". Fix in the migration file:
  table.CheckConstraint("ck_audit_logs_action", "\"Action\" IN ('Created','Updated','Deleted')");
```

---

## Phase 12 — dotnet new Template

```
Turn the solution into a dotnet new custom template so any project can be
scaffolded with: dotnet new modularmonolith -n YourProject.Name

1. Create .template.config/template.json at solution root:
   {
     "shortName": "modularmonolith",
     "sourceName": "ModularMonolith",     ← replaced everywhere by -n value
     "preferNameDirectory": true,
     "tags": { "language": "C#", "type": "solution" },
     "guids": [ ...all 5 solution GUIDs... ],  ← regenerated per project
     "sources": [{
       "exclude": ["**/bin/**","**/obj/**","**/.vs/**","**/.git/**","**/*.user",".template.config/**"]
     }]
   }

2. Install locally:
   dotnet new install D:\ModularMonolith

3. Use:
   dotnet new modularmonolith -n Acme.Inventory
```

---

## Phase 13 — NuGet Publishing

```
Package and publish the template to NuGet so anyone can install it with:
  dotnet new install BosFramework.ModularMonolith.Template

1. Create template-pack/ModularMonolith.Template.csproj:
   <PackageId>BosFramework.ModularMonolith.Template</PackageId>
   <PackageType>Template</PackageType>
   <IncludeContentInPack>true</IncludeContentInPack>
   <IncludeBuildOutput>false</IncludeBuildOutput>
   <ContentTargetFolders>content</ContentTargetFolders>
   <NoDefaultExcludes>true</NoDefaultExcludes>        ← required for .template.config
   <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
   <PackageReadmeFile>README.md</PackageReadmeFile>
   Content Include="../**" (exclude bin/obj/.git/.vs/template-pack itself)

2. Create template-pack/README.md (shown on nuget.org)

3. Pack:
   dotnet pack template-pack/ModularMonolith.Template.csproj -o nupkg

4. Push:
   dotnet nuget push nupkg/BosFramework.ModularMonolith.Template.1.0.0.nupkg \
     --api-key YOUR_NUGET_API_KEY \
     --source https://api.nuget.org/v3/index.json
```

---

## Quick Reference — Prompt Sequence

| # | Prompt (short form) |
|---|---|
| 1 | Create .NET 9 Modular Monolith solution structure |
| 2 | Implement DDD building blocks (Entity, AggregateRoot, ValueObject, events, exceptions) |
| 3 | Implement shared application abstractions (ICurrentUser, ApiResponse, pipeline) |
| 4 | Implement shared infrastructure (middleware, filters, interceptors, options) |
| 5 | Build the Auth module (domain, application, infrastructure, presentation) |
| 6 | Build the Catalog module (same layered pattern as Auth) |
| 7 | Wire Program.cs — modules, JWT, Serilog, middleware pipeline |
| 8 | Implement Swagger to document all APIs |
| 9 | Add rate limiting (fixed window for auth, sliding window for API endpoints) |
| 10 | Split rate limiting / Swagger / JWT into separate extension files |
| 11 | Create Dockerfile and docker-compose, auto-apply migrations on startup |
| 12 | Create dotnet new custom template (.template.config/template.json) |
| 13 | Package and publish to NuGet (template-pack project + dotnet nuget push) |
