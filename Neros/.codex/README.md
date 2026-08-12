# Codex Bridge - Neros

Codex debe leer `../CODEX.md`, `../AGENTS.md` y `../CONVENTIONS.md` antes de cambios no triviales.

## Agents

Usar como perfiles reutilizables:

- Planner: `../.vscode/agents/planner.md`.
- Dev: `../.vscode/agents/dev.md`.
- Backend: `../.vscode/agents/backend.md`.
- Frontend: `../.vscode/agents/frontend.md`.
- QA: `../.vscode/agents/qa.md`.

## Skills

Las skills viven en `../skills/*/SKILL.md`. Cargar la skill mas especifica antes de implementar.

## Reglas

- No ejecutar local si el usuario lo prohibe.
- No Bootstrap/jQuery/DataTables/Select2.
- Mantener Blazor -> API -> Application -> Domain.
- SQL Server con scripts manuales.