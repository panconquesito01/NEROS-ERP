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

- Database schema changes use manual SQL Server scripts in `database/scripts/`.
- Prefer idempotent scripts and rollback notes for risky changes.
- Keep migrations out unless explicitly approved for a specific workflow.

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