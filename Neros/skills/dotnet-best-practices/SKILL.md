---
name: dotnet-best-practices
description: "Use for .NET 10 best practices in Neros: Blazor Web App, ASP.NET Core Web API, Clean Architecture, DI, options, logging, validation, errors, tests, and layer boundaries."
---

# .NET Best Practices - Neros

## Architecture

- Keep dependency direction inward: Blazor/API depend on Application/Contracts, Domain depends on nothing infrastructure-specific.
- Controllers/endpoints are thin: validate request shape, authorize, call Application, map response.
- Application owns use cases, ports, validation orchestration and transaction intent.
- Domain owns business invariants and pure rules.
- Contracts owns DTOs crossing UI/API boundaries.

## Dependency Injection

- Register abstractions from Application and implementations from infrastructure/API composition root.
- Avoid duplicate registrations and service locator patterns.
- Use typed `HttpClient` clients for Blazor/API integration when needed.

## Configuration

- Bind options classes for non-trivial settings.
- Validate options on startup for required values.
- No secrets in repo; use User Secrets, environment variables or secret manager.

## Errors

- Return consistent API error responses.
- Log exceptions once at the boundary with useful context, never secrets.
- Do not swallow exceptions in background jobs, SignalR hubs or fire-and-forget tasks.

## Validation

- UI validates for user feedback.
- API validates request shape and authorization.
- Application validates business workflow.
- Domain enforces invariants.

## Frontend

- Blazor Web App + Tailwind CSS.
- No Bootstrap, jQuery, DataTables or Select2.
- Components should have clear loading/error/empty states.

## Data

- SQL Server changes use versioned scripts in `database/<modulo>/migrations/` applied by `tools/Neros.Database.Deploy`; no EF Migrations or `EnsureCreated()` (ADR-0004).
- EF Core is allowed as ORM/query mapper, not the only schema source of truth.
- Use parameterized SQL and bounded queries.

## Validation Commands

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
```

## Implementation Workflow

1. Start from the behavior and identify the owning layer.
2. Define contracts before wiring UI/API.
3. Keep the first change vertical and testable.
4. Register DI at the composition root only after an abstraction/implementation pair exists.
5. Add validation and error paths while building, not as a final afterthought.
6. Run the narrowest validation command when local execution is allowed.

## Common Neros Code Smells And Fixes

| Smell | Why It Hurts | Fix |
|---|---|---|
| Blazor component contains business calculation | Rule cannot be reused or audited | Move rule to Domain/Application and keep UI state only |
| API controller has a long workflow | Hard to test, auth/data rules scatter | Create Application use case and keep controller thin |
| DTO exposes entity shape directly | Internal model leaks to clients | Create explicit request/response records in Contracts |
| Application calls concrete filesystem/HTTP/SQL | Core depends on infrastructure | Define port in Application and implement outside core |
| Duplicate DI registrations | Last registration wins unexpectedly | Register once near related composition root |
| Options read directly from configuration everywhere | Validation and defaults scatter | Bind validated options classes |
| Catch-all exception returns raw message | Leaks internals | Log internally and return consistent safe error |
| UI uses invisible permission only | User can still call API | Enforce authorization server-side |

## API Response Pattern

- Successful command: return a typed response with the new state or identifier.
- Validation failure: return structured validation errors.
- Not found/resource forbidden: do not reveal whether another tenant owns the resource.
- Unexpected failure: log once and return a generic problem response.

## DI Checklist

- Lifetime matches dependency graph.
- Scoped services are not captured by singletons.
- Typed clients use `IHttpClientFactory`.
- Background jobs create scopes per execution.
- SignalR hubs remain thin and delegate work.

## Testing Guidance

- Domain rules: pure unit tests.
- Application use cases: fake ports, validate success/failure paths.
- API: endpoint/controller tests when behavior is security or mapping sensitive.
- Blazor: component tests when state/validation is non-trivial.
- SQL: script review plus verification query; integration test when persistence exists.