---
name: diagnose
description: "Use when diagnosing failures in Neros: build errors, Razor/Blazor issues, API/Application behavior, layer violations, Tailwind output, Graphify staleness, or migration regressions."
---

# Diagnose - Neros

## Triage Order

1. Identify the failing behavior, command, diagnostic, file or symbol.
2. Read the nearest owning code, not the whole repo.
3. State one falsifiable hypothesis and one cheap check.
4. Validate with the narrowest available signal.

## Common Failure Surfaces

- Razor component compile errors: `Neros.Blazor/Components/**` and `_Imports.razor`.
- Tailwind missing styles: `Neros.Blazor/Styles/tailwind.css`, `tailwind.config.js`, generated `wwwroot/app.css`.
- API routing/DI: `Neros.Api/Program.cs` and controllers/endpoints.
- Layering: project references and `using` directives.
- Contracts mismatch: DTOs in `Neros.Contracts` used by Blazor/API.
- Graphify stale: `graphify-out/GRAPH_REPORT.md` built from old commit.

## Diagnostic Playbooks

### Blazor Page Fails

1. Check the `.razor` page, route directive and layout.
2. Check `_Imports.razor` for missing namespaces.
3. Check component state and event handlers for null/async issues.
4. Check Tailwind only after Razor diagnostics are clean.

### API Endpoint Fails

1. Confirm route and HTTP method.
2. Confirm DI registration for the use case/service.
3. Confirm request DTO binding and validation.
4. Confirm authorization path and resource scope.
5. Confirm Application result mapping.

### Layer Violation Suspected

1. Inspect `.csproj` references.
2. Search `using Microsoft.EntityFrameworkCore`, ASP.NET or Blazor namespaces in Domain/Application.
3. Check whether a DTO or entity crossed the wrong boundary.
4. Move the dependency inward via a port or contract.

### Tailwind Style Missing

1. Confirm class appears in `.razor`, `.html` or `.cshtml` covered by `tailwind.config.js` content.
2. Confirm `Neros.Blazor/Styles/tailwind.css` contains required component classes.
3. Confirm generated `wwwroot/app.css` is expected to be rebuilt.
4. If commands are forbidden, leave `npm run css:build` pending.

## Error Pattern Table

| Symptom | Likely Cause | Cheap Check |
|---|---|---|
| `RZ...` Razor error | Component syntax/import/layout issue | Editor diagnostics on touched `.razor` |
| `CS0246` missing type | Namespace/project reference missing | `_Imports.razor`, `using`, `.csproj` |
| Service not resolved | DI registration missing or wrong lifetime | `Program.cs` in API/Blazor |
| Route 404 | Wrong `@page`, controller route, base href | Route declaration and nav link |
| UI has no styles | Tailwind output stale or class not scanned | `tailwind.config.js` and CSS build |
| Cross-tenant data visible | Missing scope predicate/authorization | Query/use case filter |

## Legacy Smell Checks

Flag these if they appear in new work:

- Bootstrap, jQuery, DataTables, Select2.
- MVC Areas or `.cshtml` as new UI.
- `Servicios/*` copied as target architecture.
- DbContext access from Blazor.
- Business logic in API controllers.

## If Local Commands Are Allowed

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

## If Local Commands Are Not Allowed

- Use editor diagnostics.
- Use targeted text searches.
- Read generated/stale artifacts only as evidence, not as source of truth.
- Tell the user which commands remain pending.

## Output

```text
Observed:
Hypothesis:
Check:
Finding:
Fix:
Validation:
```

## Stop Conditions

- The hypothesis is falsified and the owning layer changes.
- A security issue is discovered; switch to `security-review`.
- A schema change is required; switch to `ef-core` / SQL script planning.
- The issue is architectural; switch to `architecture-blueprint-generator` or `create-implementation-plan`.