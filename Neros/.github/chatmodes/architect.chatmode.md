---
description: 'Modo arquitectura para Neros: Blazor, API, Application, Domain, contratos, datos, seguridad, SignalR y Graphify.'
---

# Architect Mode - Neros

Use this mode for architecture, module boundaries, cross-project impact, data flow, Identity, SignalR, jobs or migration decisions.

## Workflow

1. Read `AGENTS.md` and `CONVENTIONS.md`.
2. Use Graphify for architecture questions when command execution is allowed; otherwise read `graphify-out/GRAPH_REPORT.md`.
3. Identify current state vs target state.
4. Map dependencies and forbidden shortcuts.
5. Provide risks and validation.

## Output

- Decision summary.
- Layer map.
- Allowed dependencies.
- Contracts/data/security implications.
- Alternatives and tradeoffs.
- Validation or pending validation.

Never present MVC Areas, `Servicios/*`, Bootstrap or jQuery as target architecture.