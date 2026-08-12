---
name: create-implementation-plan
description: "Use when planning a Neros feature, migration, fix, endpoint, Blazor workflow, SQL change, Identity/SignalR work, or cross-layer refactor before implementation."
---

# Create Implementation Plan - Neros

Create a practical, layer-aware plan that an implementation agent can execute without re-discovering the architecture.

## Preflight

1. Read `AGENTS.md` and `CONVENTIONS.md`.
2. Consult Graphify for architecture/impact when available and allowed.
3. Identify whether this is new Neros work or legacy functional migration.
4. Find the owning layer and one nearby implementation example.

## Plan Template

```text
# Plan: <feature/fix>

## Objective
<business outcome in Spanish>

## Scope
- In: <files/projects/layers>
- Out: <explicit non-goals>

## Current Facts
- <facts from code/Graphify>

## Layer Design
- Blazor: <pages/components/state/validation>
- Contracts: <request/response DTOs>
- Api: <endpoint/auth/validation>
- Application: <use case/ports/transactions>
- Domain: <entities/value objects/rules>
- Data: <SQL script or none>

## Steps
1. <small testable step>
2. <small testable step>

## Security/Data Risks
- <permissions, secrets, audit, tenant/resource scope, concurrency>

## Validation
- <commands or editor diagnostics>
```

## Rules

- Prefer small vertical slices over broad refactors.
- Do not place business rules in Blazor or controllers.
- Do not invent infrastructure unless a port/use case needs it.
- For database changes, include a manual SQL script plan with rollback notes.
- For UI, include loading, error, empty and validation states.
- For SignalR, include hub, group strategy, payload contract and authorization.

## Planning Heuristics

- If the UI needs new data, define the Contract and API before polishing UI.
- If the behavior changes state, define idempotency and transaction boundary.
- If a rule is reusable or auditable, put it in Domain/Application, not Blazor.
- If persistence is required, plan the manual SQL script before writing code that assumes schema exists.
- If a feature touches permissions, plan server-side policy and audit first.
- If a legacy workflow is being migrated, extract functional rules before mapping files.

## Common Plan Gaps To Avoid

| Gap | Add This |
|---|---|
| "Create page" only | Contract/API/Application path and empty/error/loading states |
| "Add table" only | SQL script, indexes, constraints, rollback and query owner |
| "Add endpoint" only | Auth policy, request/response DTOs and result mapping |
| "Use SignalR" only | Hub location, group naming, payload shape, reconnect behavior |
| "Migrate legacy module" only | Legacy behavior summary and new-layer mapping |

## Acceptance Criteria

- Behavior is described in business terms.
- Each layer has a clear owner or is explicitly out of scope.
- Security/data risks are named.
- Validation is executable or marked pending due to user constraint.
- The plan can be implemented in small commits/slices.

## Output

Return the plan only. Do not implement unless the user asks or the current agent workflow says to proceed.