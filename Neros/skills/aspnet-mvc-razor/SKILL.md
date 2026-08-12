---
name: aspnet-mvc-razor
description: "Legacy-only reference for MVC/Razor Areas from the previous system. Use only when the user explicitly asks to inspect or migrate old MVC/Razor behavior into Neros; do not use for new UI."
---

# ASP.NET MVC/Razor Legacy Reference - Neros

This skill exists to understand old MVC/Razor behavior during migration. It is not the target UI architecture.

## Target For New Work

- New UI lives in `Neros.Blazor` as Blazor Web App components/pages.
- Styling uses Tailwind CSS.
- Data access goes through API/Application, never from Blazor.
- Shared request/response types live in `Neros.Contracts`.

## When To Use

Use only if the user asks for:

- Migrating a specific old MVC/Razor workflow.
- Explaining old `.cshtml`, controller, Area or service behavior.
- Translating old functional rules into new Neros layers.

## Migration Pattern

```text
Old MVC Controller action -> API endpoint + Application use case
Old ViewModel/Form -> Contract request/response DTO
Old Razor view -> Blazor page/component + Tailwind
Old repository/service logic -> Application port + Domain rule + infrastructure implementation later
Old SQL/EF schema change -> manual SQL script in database/scripts/
```

## Legacy Extraction Workflow

1. Identify the old route, controller action, view and service involved.
2. Extract user-visible behavior, inputs, validations and side effects.
3. Extract business rules separately from UI mechanics.
4. Identify data reads/writes and security checks.
5. Map each item to the Neros target layers.
6. Produce a migration plan before coding.

## What To Preserve

- Business validations.
- Permission intent.
- Audit requirements.
- Data consistency rules.
- User workflow and terminology.
- Known edge cases and historical bug fixes.

## What To Replace

| Legacy Shape | Neros Shape |
|---|---|
| MVC Area navigation | Blazor routes/layout/nav |
| Razor partial + AJAX | Blazor component state/service call |
| Controller transaction logic | Application use case transaction boundary |
| ViewModel tightly coupled to view | Contract request/response DTO |
| jQuery/DataTables behavior | Blazor/Tailwind component behavior |
| EF migration-first schema | Manual SQL script plus future EF mapping |

## Common Migration Traps

- Copying route names before understanding the business action.
- Recreating old UI widgets instead of designing an ERP workflow in Blazor.
- Trusting old client-side validation as complete.
- Carrying old table/entity names into public contracts unnecessarily.
- Migrating a service method as-is without separating Domain rules.

## Do Not

- Do not create new MVC Areas.
- Do not add new `.cshtml` views for Neros UI.
- Do not introduce Bootstrap, jQuery, DataTables or Select2.
- Do not copy direct DbContext usage into Blazor or API controllers.

## Output

When migrating legacy behavior, return:

- Old behavior summary.
- New Neros layer mapping.
- Contracts required.
- Data/security implications.
- Small implementation plan and validation.

## Escalate To

- `create-implementation-plan` for the actual Neros implementation plan.
- `ef-core` if schema or query behavior is involved.
- `security-review` if old permissions or tenant filters are unclear.