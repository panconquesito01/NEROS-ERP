---
description: 'C# conventions for Neros layered .NET projects.'
applyTo: '**/*.cs'
---

# C# - Neros

- Use .NET 10 and nullable reference types.
- Keep business names in Spanish when they represent Neros concepts.
- Prefer small, explicit types over anonymous cross-layer contracts.
- Use primary constructors only when they improve readability.

## Layers

- Domain stays pure: no EF Core, ASP.NET, Blazor, HTTP, filesystem or UI dependencies.
- Application contains use cases, ports, validation orchestration and transaction intent.
- API endpoints/controllers stay thin and delegate to Application.
- Contracts contains DTOs crossing UI/API boundaries.
- Shared contains small, dependency-light primitives.

## Security And Data

- Never concatenate user input into SQL.
- Do not log secrets, tokens, connection strings or full request bodies.
- Validate external input at API/Application boundaries.
- Enforce resource authorization server-side.

## Dependency Injection

- Register abstractions and implementations explicitly.
- Avoid service locator patterns.
- Keep infrastructure implementations outside Domain/Application core.

## After Editing

```powershell
dotnet build .\Neros.slnx -v minimal
```