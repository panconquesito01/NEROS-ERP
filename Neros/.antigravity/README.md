# Antigravity Bridge - Neros

Antigravity debe leer `../ANTIGRAVITY.md`, `../AGENTS.md` y `../CONVENTIONS.md` antes de cambios no triviales.

## Agents

Reutilizar los perfiles canonicos:

- Planner: `../.vscode/agents/planner.md`.
- Dev: `../.vscode/agents/dev.md`.
- Backend: `../.vscode/agents/backend.md`.
- Frontend: `../.vscode/agents/frontend.md`.
- QA: `../.vscode/agents/qa.md`.

## Skills

Las skills viven en `../skills/*/SKILL.md` y definen workflows robustos para arquitectura, diagnostico, SQL, seguridad, refactor y review.

## Reglas

- No ejecutar local si el usuario lo prohibe.
- No copiar arquitectura legacy del monolito.
- No Bootstrap, jQuery, DataTables ni Select2.
- Mantener separacion por capas.