---
name: ef-core
description: "Use for EF Core and SQL Server work in Neros: future persistence, DbContext design, queries, mappings, transactions, manual SQL scripts, performance, and data safety."
---

# EF Core / SQL Server - Neros

Neros has not committed to a persistence project as the source of schema truth. When persistence is added, EF Core maps and queries SQL Server while schema changes are documented as manual scripts.

## Placement

- Domain does not reference EF Core.
- Application defines ports/repository abstractions only when a use case needs them.
- Infrastructure/Persistence implements those ports.
- API composes DI and transaction behavior.
- Blazor never uses DbContext.

## Schema Changes

- Prefer manual SQL Server scripts in `database/scripts/`.
- Scripts should be named with order/date and intent.
- Make scripts idempotent where practical.
- Include rollback notes or compensating script when risky.
- Keep seed/reference data explicit.

## Query Rules

- Use async EF APIs for I/O.
- Use `AsNoTracking` for read-only queries.
- Project to DTO/query models instead of returning entities across boundaries.
- Avoid lazy loading surprises.
- Avoid N+1 queries; use projection or explicit includes.
- Parameterize raw SQL.

## Transactions

- Transaction boundary belongs to Application use case or infrastructure unit of work, not Blazor.
- Keep external HTTP calls outside database transactions where possible.
- Make job/retry operations idempotent.

## Workflow

1. Identify the use case that needs persistence and define the Application port first.
2. Decide whether the operation is command, query, report or background job.
3. Design the SQL Server shape with constraints, indexes and audit needs.
4. Create or update the manual SQL script under `database/scripts/`.
5. Implement EF Core mapping/query code in the future persistence project.
6. Project outward to DTOs; never leak entities to API/Blazor contracts.
7. Validate performance, transaction scope and rollback story.

## Manual SQL Script Template

```sql
-- Purpose: <business change>
-- Preconditions: <tables/config expected>
-- Rollback: <rollback script or mitigation>

IF NOT EXISTS (
	SELECT 1
	FROM sys.tables
	WHERE name = N'<TableName>'
	  AND schema_id = SCHEMA_ID(N'dbo')
)
BEGIN
	CREATE TABLE dbo.<TableName> (
		Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_<TableName> PRIMARY KEY,
		CreadoEn datetime2(0) NOT NULL CONSTRAINT DF_<TableName>_CreadoEn DEFAULT SYSUTCDATETIME()
	);
END;
```

## Common Mistakes And Fixes

| Mistake | Risk | Better |
|---|---|---|
| DbContext injected into Blazor | UI bypasses API/Application, leaks data rules | Blazor calls typed client/API; API delegates to Application |
| Domain type has EF/ASP.NET attributes by default | Domain stops being pure | Keep mapping in Fluent API configuration |
| Returning `IQueryable<T>` across a port | EF concerns leak outward | Materialize inside persistence and return DTO/read model |
| `Task.WhenAll` over operations sharing one DbContext | DbContext is not thread-safe | Use sequential work or separate scopes with bounded concurrency |
| `ExecuteSqlRaw($"... {input}")` | SQL injection | Use interpolated/parameterized APIs |
| Unbounded grid query | Memory/timeouts | Add filters, pagination and deterministic ordering |
| External API call inside DB transaction | Locks held while network waits | Persist intent, commit, call external service, reconcile |

## Query Examples

```csharp
// Good: read-only projection with bounded result.
var items = await db.Set<Pedido>()
	.AsNoTracking()
	.Where(p => p.EmpresaId == empresaId && p.Estado == estado)
	.OrderByDescending(p => p.Fecha)
	.Take(100)
	.Select(p => new PedidoResumenDto(p.Id, p.Numero, p.Fecha, p.Total))
	.ToListAsync(cancellationToken);
```

```csharp
// Bad: raw SQL concatenates input and returns entity outside persistence.
var sql = "SELECT * FROM Pedidos WHERE Numero = '" + numero + "'";
```

## Performance Checklist

- Is there a selective index for the filter/order?
- Is the query projected before materialization?
- Is pagination enforced for grids/search?
- Is tracking disabled for read-only queries?
- Are includes necessary, or would projection be cheaper?
- Is tenant/company scope part of the predicate?

## Validation Checklist

- SQL script reviewed for idempotency and rollback.
- Raw SQL is parameterized.
- No DbContext crosses into Blazor, Domain or Contracts.
- Transaction boundary is documented.
- Query cardinality is bounded.
- Sensitive data is not logged or exposed in DTOs.

