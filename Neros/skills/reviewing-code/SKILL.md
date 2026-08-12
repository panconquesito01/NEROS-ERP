---
name: reviewing-code
description: "Use when reviewing Neros changes for correctness, architecture, security, data safety, Blazor/Tailwind regressions, tests, build risk, Graphify impact, and legacy-pattern leakage."
---

# Reviewing Code - Neros

Review like a senior engineer. Findings first, ordered by severity.

## Review Checklist

- Correctness: does the change satisfy the requested behavior?
- Layering: Blazor -> API -> Application -> Domain; no forbidden references.
- Contracts: DTOs explicit, no internal entities or sensitive fields exposed.
- Security: authn/authz, resource scoping, secrets, audit, input validation.
- Data: SQL parameterized, transaction boundary clear, manual script for schema changes.
- Async: cancellation, no sync-over-async, no unsafe parallel DbContext usage.
- UI: Tailwind only, responsive, accessible, loading/error/empty states.
- SignalR/jobs: authorization, idempotency, retries, group/payload correctness.
- Legacy leakage: no Bootstrap, jQuery, DataTables, Select2, MVC Areas as new UI, `Servicios/*` as target architecture.
- Validation: tests/build/CSS/Graphify status documented.

## Severity Rubric

- Critical: data leak/corruption, auth bypass, secret exposure, destructive operation, cross-tenant access.
- High: broken core workflow, missing server-side authorization, unsafe SQL, transaction inconsistency.
- Medium: layer violation, missing validation, brittle async, incomplete error handling, UI state regression.
- Low: naming, minor maintainability, missing non-blocking docs.

## Common Review Findings In Neros

| Finding | What To Check |
|---|---|
| Blazor calls persistence directly | Component injections, project references, service methods |
| Controller owns business rule | Long action body, calculations, transaction logic |
| DTO returns too much | Sensitive fields, internal IDs, navigation properties |
| Missing tenant/resource scope | Queries and use case inputs include company/sucursal/resource |
| UI-only permission check | API/Application enforces the same rule |
| SignalR broadcasts too much | Payload contains full entity/secret or ignores groups |
| Schema change lacks script | `database/scripts/` has matching manual SQL |
| CSS relies on Bootstrap class | New UI class names and dependencies |

## Review Procedure

1. Read the diff or touched files.
2. Identify changed behavior and owning layers.
3. Trace one representative request/UI flow end-to-end.
4. Check security/data boundaries before style issues.
5. Verify validation evidence; if local commands were forbidden, mark as pending.
6. Report only actionable findings with impact and fix.

## Output Format

```text
Findings
- [Severity] File: issue, impact, suggested fix.

Open Questions
- <only if blocking or material>

Validation
- <what ran or what remains pending>
```

If there are no findings, say so clearly and mention residual risk.

## Avoid

- Do not list generic best practices without tying them to code.
- Do not request broad rewrites when a small fix closes the risk.
- Do not treat generated `bin/obj` artifacts as source findings unless they are versioned unexpectedly.