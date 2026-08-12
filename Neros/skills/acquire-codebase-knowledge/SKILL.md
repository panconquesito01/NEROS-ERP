---
name: acquire-codebase-knowledge
description: "Use when you need to understand Neros before coding: architecture, projects, layer ownership, Graphify context, current conventions, commands, risks, or migration boundaries."
---

# Acquire Codebase Knowledge - Neros

Use this skill before non-trivial work in `Neros`, especially when a task touches more than one project, architecture, security, data flow, Identity, SignalR, SQL Server, or migration from the previous system.

## Goal

Build a short, factual working model of the current Neros codebase before changing it.

## Required Sources

Read in this order:

1. `AGENTS.md` and `CONVENTIONS.md`.
2. `graphify-out/GRAPH_REPORT.md` or `graphify query "<question>"` when local execution is allowed.
3. `Neros.slnx` and the touched `.csproj` files.
4. The owning implementation surface: Blazor page/component, API endpoint, Application use case, Domain rule, Contract DTO, or Shared type.
5. Neighboring examples in the same layer.

## Architecture Map

```text
Neros.Blazor -> Neros.Api -> Neros.Application -> Neros.Domain
Neros.Blazor -> Neros.Contracts / Neros.Shared
Future Infrastructure/Persistence -> implements Application ports
```

## What To Capture

- User-facing behavior or business flow.
- Owning layer and why it owns the behavior.
- Contracts crossing process/layer boundaries.
- Data and transaction boundary.
- Security boundary: authentication, authorization, tenant/resource scoping, audit.
- UI boundary: Blazor state, validation, loading/error states, Tailwind constraints.
- Existing validation command or editor diagnostic to use.

## Exploration Workflow

1. Translate the user's request into a concrete behavior or architecture question.
2. Use Graphify first for architecture/impact if command execution is allowed.
3. If commands are forbidden, read `graphify-out/GRAPH_REPORT.md` and the nearest source files.
4. Identify the owning layer and one neighboring example.
5. Stop exploring once you can name one hypothesis and one validation check.

## Layer Ownership Guide

| Question | Owning Layer |
|---|---|
| What does the user see/click/type? | Blazor |
| What crosses UI/API? | Contracts |
| What route/auth boundary receives the request? | Api |
| What business action is performed? | Application |
| What invariant must always be true? | Domain |
| What query/storage/external system is needed? | Infrastructure/Persistence future |

## Evidence To Prefer

- Existing code in the same layer over broad assumptions.
- Project references over folder names.
- Contracts and DTOs over inferred payload shapes.
- Graphify report/query for cross-file relationships.
- Editor diagnostics over generated `bin/obj` artifacts.

## Common Discovery Mistakes

| Mistake | Better Move |
|---|---|
| Reading the whole repo before forming a hypothesis | Read the owner and one neighbor |
| Treating legacy names as target architecture | Mark them as functional reference only |
| Starting from UI and skipping Contracts/API | Trace the boundary explicitly |
| Trusting generated `obj` paths | Check source `.csproj` and source files |
| Ignoring Graphify when architecture is the question | Query/path/explain first when allowed |

## Migration Rule

The old system is functional reference only. Do not copy MVC Areas, `Servicios/*`, monolithic repositories, direct DbContext access from UI, Bootstrap, jQuery, DataTables or Select2 into Neros.

## Output

```text
Context:
- <facts learned>

Owning layer:
- <project/file/symbol>

Contracts/data/security:
- <DTOs, persistence, auth/audit implications>

Validation:
- <command or static diagnostic>

Risks:
- <open risks or none>
```

## Escalate To

- `architecture-blueprint-generator` for durable docs or architecture diagrams.
- `create-implementation-plan` before multi-layer implementation.
- `diagnose` when there is a concrete failure.
- `security-review` when auth/data exposure is involved.