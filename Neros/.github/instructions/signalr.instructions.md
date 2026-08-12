---
description: 'SignalR hub and client conventions for Neros.'
applyTo: '**/*Hub*.cs, **/Hubs/**/*.cs, **/*SignalR*.cs, **/wwwroot/**/*.js'
---

# SignalR - Neros

SignalR is for operational events, notifications, progress updates and collaborative workflows. It is not a replacement for API queries.

## Server

- Hubs live in API or an infrastructure-facing host, not in Blazor.
- Apply authentication and server-side authorization.
- Validate resource access inside hub methods; `[Authorize]` alone is not enough for resource-scoped actions.
- Use typed hubs when client contract is stable.
- Keep hub methods thin and delegate business work to Application.
- Do not store per-connection mutable state in instance fields.

## Groups

- Use explicit group names by user, tenant, sucursal, module or resource.
- Join groups only after authorization.
- Remove or let disconnect cleanup handle short-lived groups.

## Payloads

- Keep messages small.
- Send IDs/status events and let clients fetch full data over HTTP.
- Never send secrets, raw SQL rows or sensitive internal entities.

## Client

- Show reconnecting/reconnected/closed states.
- Avoid duplicating event handlers on component rerender.
- Dispose connections/subscriptions when the component leaves.

## Validation

- Verify authorization path.
- Verify group membership rules.
- Verify reconnect behavior and duplicate handler prevention.