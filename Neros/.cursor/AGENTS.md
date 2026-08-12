# Cursor Agents - Neros

Misma convencion que `.vscode/AGENTS.md`. Los agentes Cursor deben tratar `../AGENTS.md` como fuente de verdad del proyecto.

## Proyecto

- Blazor Web App + Tailwind CSS para frontend.
- ASP.NET Core Web API para endpoints.
- `Application`, `Domain`, `Contracts` y `Shared` como capas core.
- SQL Server con scripts manuales versionados cuando haya persistencia.
- Identity, SignalR y jobs se agregan respetando puertos de Application.

## Reglas

- No Bootstrap, jQuery, DataTables ni Select2.
- No MVC Areas/Servicios del monolito como arquitectura objetivo.
- Blazor no accede a datos.
- API delega en Application.
- Domain permanece puro.
- Consultar Graphify antes de cambios arquitectonicos cuando sea posible.

## Puentes Compatibles

- Codex: `../CODEX.md` y `../.codex/AGENTS.md`.
- Antigravity: `../ANTIGRAVITY.md` y `../.antigravity/AGENTS.md`.
- Claude: `../CLAUDE.md`.
- Gemini: `../GEMINI.md`.

Los perfiles `.cursor/agents/*.md` deben mantenerse sincronizados con `.vscode/agents/*.md`.

