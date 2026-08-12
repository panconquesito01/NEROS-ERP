---
name: security-review
description: "Use for security reviews in Neros: Identity, authorization, tenant/resource scoping, secrets, SQL injection, XSS, CSRF, SignalR hubs, file/report generation, jobs, and dependency risks."
---

# Security Review - Neros

## Scope

Review the changed files or requested module first. Expand only when a security boundary depends on nearby code.

## Neros Security Model

- Identity is the authentication foundation.
- Authorization should be role/resource/policy based, not just UI visibility.
- Tenant, empresa, sucursal or resource scope must be enforced server-side when applicable.
- API is the primary boundary for Blazor.
- SignalR hubs are server endpoints and need the same authorization discipline as API.

## Checklist

- Secrets: no passwords, tokens, connection strings or keys in repo.
- Authentication: endpoints/hubs that need users require auth.
- Authorization: server-side policy checks for actions and resources.
- Input: validate request DTOs at API/Application boundary.
- SQL: parameterized queries only; no concatenated user input.
- XSS: render user content safely; avoid raw markup unless sanitized.
- CSRF: consider state-changing browser endpoints and auth cookies.
- File/report output: path traversal, content type, size limits, temp file cleanup.
- Logging: no secrets or sensitive payloads.
- Jobs/retries: idempotency and least-privilege service accounts.
- Dependencies: flag known vulnerable packages and risky transitive usage.

## Review Workflow

1. Define the protected asset: data, operation, file, report, hub, token or role.
2. Identify entry points: Blazor action, API endpoint, hub method, job, import, report generation.
3. Trace identity and authorization from entry point to Application/data access.
4. Trace input validation and output shaping.
5. Check logging and error paths.
6. Check persistence/query boundaries and tenant/resource filters.
7. Report only concrete exploit paths or credible misuse paths.

## Neros Threat Patterns

| Pattern | Risk | Expected Control |
|---|---|---|
| UI hides a button but API lacks policy | User calls endpoint directly | Server-side authorization in API/Application |
| Query lacks `EmpresaId`/tenant scope | Cross-company data leak | Scope enforced in use case/query |
| SignalR sends full entity to group/all | Data leak to wrong users | Authorized group membership and minimal payload |
| Report path built from input | Path traversal | Allowlisted template names and safe storage |
| Raw HTML from user content | XSS | Safe rendering or sanitization |
| Retryable command creates duplicates | Double processing | Idempotency key or unique constraint |
| Logs include request body | PII/secret leak | Structured safe logging |

## Identity And Authorization Checks

- Login endpoints should have rate limiting and safe error messages.
- Password reset and invite flows must use one-time, expiring tokens.
- Role checks should not replace resource checks for tenant-scoped data.
- Admin actions require audit trail.
- Background jobs should run with explicit service identity assumptions.

## SQL And Data Checks

- Parameterized SQL only.
- No unbounded export without authorization and filters.
- No soft-deleted/closed records modified without explicit rule.
- No physical delete of audited business documents unless the domain permits it.
- Transactions should not hide partial failure or duplicate side effects.

## Output

```text
Findings
- [Critical/High/Medium/Low] <file>: <issue> -> <impact> -> <fix>

Residual Risk
- <what remains unknown>

Validation
- <commands/checks run or pending>
```

Do not report theoretical issues without a plausible exploit path or concrete risk in the code.

## Escalate To

- `ef-core` for schema/query/transaction fixes.
- `dotnet-best-practices` for DI/options/error handling fixes.
- `reviewing-code` for broad PR review after security fixes.