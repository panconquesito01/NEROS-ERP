# CODEX.md

Guia para Codex en `Neros`. Leer `AGENTS.md` y `CONVENTIONS.md` antes de cambios no triviales.

## Prioridad

- Fuente principal: `AGENTS.md`.
- Convenciones: `CONVENTIONS.md`.
- Skills locales: `skills/*/SKILL.md`.
- Agentes equivalentes: `.vscode/agents/*.md` y `.cursor/agents/*.md`.
- Grafo: `graphify-out/graph.json` cuando exista y se permita ejecutar comandos.

## Arquitectura Objetivo

```text
Neros.Blazor -> Neros.Api -> Neros.Application -> Neros.Domain
Neros.Blazor -> Neros.Contracts / Neros.Shared
Future Infrastructure/Persistence -> Application ports
```

## Reglas Duras

- No usar `Neros.Next`, Aurosoft o Equaltech como arquitectura objetivo.
- No copiar MVC Areas, `Servicios/*` ni patrones del monolito anterior salvo como referencia funcional.
- No introducir Bootstrap, jQuery, DataTables ni Select2.
- Blazor no accede a datos ni DbContext.
- API delega en Application.
- Domain permanece puro.
- SQL Server evoluciona con scripts manuales en `database/scripts/`.
- No guardar secretos ni imprimirlos en logs/salidas.

## Seleccion De Skill

| Tarea | Skill |
|---|---|
| Entender repo/impacto | `acquire-codebase-knowledge` |
| Arquitectura | `architecture-blueprint-generator` |
| Plan de feature/fix | `create-implementation-plan` |
| Diagnostico | `diagnose` |
| Backend/.NET | `dotnet-best-practices` |
| SQL/EF/persistencia | `ef-core` |
| Async/concurrencia | `csharp-async` |
| Refactor | `refactor` |
| Review | `reviewing-code` |
| Seguridad | `security-review` |
| Migracion legacy MVC | `aspnet-mvc-razor` |

## Flujo Recomendado

1. Identificar capa propietaria.
2. Cargar skill aplicable.
3. Hacer cambios pequenos y verificables.
4. Validar con diagnosticos o comandos si el usuario lo permite.
5. Si el usuario prohibe ejecucion local, no ejecutar `dotnet`, `npm`, `graphify`, tests ni servidores.

## Comandos Canonicos

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

## Entrega

- Cambios por archivo.
- Validaciones ejecutadas o pendientes.
- Riesgos de seguridad/datos/UX.
- Handoff recomendado: planner, dev, backend, frontend o qa.