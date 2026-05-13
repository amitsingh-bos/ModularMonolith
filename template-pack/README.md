# Modular Monolith API Template

A production-ready **.NET 9** Modular Monolith starter you can scaffold in one command.

## Quick start

```bash
# Install
dotnet new install BosFramework.ModularMonolith.Template

# Scaffold
dotnet new modularmonolith -n Acme.Inventory

# Run
cd Acme.Inventory
docker compose up
```

Swagger UI is available at `http://localhost:8080` once the containers are healthy.

## What's included

| Concern | Technology |
|---|---|
| Auth | JWT Bearer — register, login, refresh, revoke |
| Rate limiting | Fixed window (auth) · Sliding window (API) |
| API docs | Swagger / OpenAPI with JWT security |
| Persistence | EF Core 9 + PostgreSQL (Npgsql) |
| Validation | FluentValidation with global action filter |
| Multi-tenancy | Scoped `ITenantContext` per request |
| Soft-delete | Global query filters + `SoftDeleteInterceptor` |
| Auditing | `IAuditLogger` with JSON old/new value capture |
| Logging | Serilog structured logging |
| API versioning | `Asp.Versioning.Mvc` 8.x |
| Containerisation | Dockerfile (multi-stage) + Docker Compose |

## Modules

- **Auth** — users, roles, permissions, refresh tokens, tenant management
- **Catalog** — products, categories, stock adjustment

## Architecture

```
src/
└── YourProject/
    ├── BuildingBlocks/          # Shared abstractions and infrastructure
    │   ├── Domain/              # AggregateRoot, Entity, ValueObject, DomainEvents
    │   ├── Application/         # Common DTOs, paged results, ApiResponse envelope
    │   └── Infrastructure/      # JWT, Swagger, rate limiting, EF interceptors
    └── Modules/
        ├── Auth/                # Domain · Application · Infrastructure · Presentation
        └── Catalog/             # Domain · Application · Infrastructure · Presentation
```

## Configuration

Edit `appsettings.json` (or override via environment variables in Docker):

```json
{
  "ConnectionStrings": {
    "Default": "Host=db;Port=5432;Database=db_YourProject;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "SecretKey": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "YourProject",
    "Audience": "YourProject",
    "ExpiryMinutes": 15
  },
  "RefreshToken": {
    "ExpiryDays": 7
  }
}
```

## License

MIT
