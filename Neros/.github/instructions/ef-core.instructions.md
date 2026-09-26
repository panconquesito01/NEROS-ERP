---
description: 'EF Core and SQL Server conventions for future Neros persistence work.'
applyTo: '**/Neros.Persistence/**/*.cs, **/database/**/*.sql, **/*DbContext*.cs, **/*Configuration*.cs'
---

# EF Core - Neros

Persistence is added as infrastructure. It must not leak into Blazor or Domain.

## Placement

- EF Core belongs in a persistence/infrastructure project when introduced.
- Application defines ports; persistence implements them.
- Domain entities should remain persistence-ignorant unless the team explicitly chooses otherwise.
- Blazor never injects DbContext or repositories.

## Schema

- Schema changes only through versioned, immutable scripts in `database/<modulo>/migrations`, applied with `tools/Neros.Database.Deploy` (ADR-0004). Follow `database/conventions/SQL_CONVENTIONS.md`.
- EF Migrations, `Database.Migrate()`, `EnsureCreated()` and `GenerateCreateScript()` are forbidden, including in tests.
- Every mapping change needs its script; the EF drift test must stay green.

## Queries

- Use async EF APIs for I/O.
- Use `AsNoTracking` for read-only queries.
- Project to DTOs/query models for API responses.
- Avoid unbounded grids/searches.
- Avoid N+1 queries.
- Raw SQL must be parameterized.

## Transactions

- Transaction boundaries belong to Application use cases or infrastructure units of work.
- Keep external HTTP calls outside DB transactions when possible.
- Make retried jobs idempotent.