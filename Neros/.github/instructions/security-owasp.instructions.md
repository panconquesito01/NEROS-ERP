---
description: 'Security rules for Neros.'
applyTo: '**'
---

# Security - Neros

- No secrets in repository files, chat output, logs or generated docs.
- Use User Secrets, environment variables or secret manager for local development secrets.
- API endpoints require authorization by default once authentication is added.
- Blazor must not access persistence directly.
- Validate and authorize file downloads server-side.
- Use parameterized SQL only.
- Add rate limiting for login and public endpoints when Identity is introduced.
- Do not log passwords, JWTs, refresh tokens, connection strings or full payloads.

## Authorization

- Enforce permissions server-side, not only in UI visibility.
- Scope data by tenant, empresa, sucursal or resource when applicable.
- SignalR hub methods need resource checks, not only `[Authorize]`.

## Input And Output

- Validate DTOs at API/Application boundaries.
- Encode or safely render user-controlled text.
- Avoid raw HTML unless sanitized.
- Set content type and download names deliberately for files.

## Review Triggers

- Authentication/login changes.
- Report/file generation.
- SQL or raw query changes.
- SignalR hubs.
- Background jobs and retries.
- Any new package that processes files, HTML, PDF or network input.