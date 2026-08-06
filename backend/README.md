# Backend

ASP.NET Core 9, Clean Architecture, CQRS via MediatR + FluentValidation,
EF Core + PostgreSQL.

## Layout

```
src/
  SaveSenseAI.Domain            Entities, zero external dependencies
  SaveSenseAI.Application       Commands/queries, validators, interfaces
  SaveSenseAI.Infrastructure    EF Core, JWT issuance, migrations
  SaveSenseAI.API               Minimal API endpoints, auth middleware (composition root)
tests/
  *.UnitTests                   No external dependencies (Domain, Infrastructure services)
  *.IntegrationTests            Run against a real Postgres database
```

Dependencies point inward only: `Domain` → nothing, `Application` → `Domain`,
`Infrastructure` → `Application`, `API` → both. See
[docs/adr/0001-monorepo-structure.md](../docs/adr/0001-monorepo-structure.md).

## What's implemented

- **Auth** — Google OAuth (backend-driven, no third-party token ever reaches
  the browser), JWT access tokens + rotating refresh tokens with reuse
  detection, `/api/auth/*`.
- **Coupons** — Stores, Coupons, and validation with a specific
  failure reason (expired / minimum spend not met / redemption limit
  reached), with every attempt persisted as an audit trail. `/api/stores/*`,
  `/api/coupons/*`.

## Local development

Requires Postgres running (see [infrastructure/](../infrastructure)) and
JWT/Google secrets set via `dotnet user-secrets` in `src/SaveSenseAI.API`
(never in `appsettings.json`):

```bash
dotnet user-secrets set "Jwt:SigningKey" "<random-64-byte-string>"
dotnet user-secrets set "Authentication:Google:ClientId" "<from Google Cloud Console>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<from Google Cloud Console>"

dotnet tool restore
dotnet tool run dotnet-ef database update --project src/SaveSenseAI.Infrastructure --startup-project src/SaveSenseAI.API

dotnet run --project src/SaveSenseAI.API
```

## Testing

```bash
dotnet test
```

Integration tests need a reachable Postgres — see `PostgresFixture` in
`SaveSenseAI.Infrastructure.IntegrationTests`, connection string overridable
via `INTEGRATION_TEST_DB_CONNECTION`.
