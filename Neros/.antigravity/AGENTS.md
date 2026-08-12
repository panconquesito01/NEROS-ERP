# Antigravity Agents - Neros

Antigravity usa los mismos perfiles canonicos que VS Code/Cursor:

- `../.vscode/agents/planner.md`
- `../.vscode/agents/dev.md`
- `../.vscode/agents/backend.md`
- `../.vscode/agents/frontend.md`
- `../.vscode/agents/qa.md`

## Flujo

```text
planner -> dev -> qa
```

## Reglas Compartidas

- Seguir `../AGENTS.md` y `../CONVENTIONS.md`.
- Cargar skills de `../skills/*/SKILL.md` segun la tarea.
- Usar Graphify para arquitectura si se permite ejecutar local.
- Dejar validaciones pendientes cuando el usuario prohibe comandos.