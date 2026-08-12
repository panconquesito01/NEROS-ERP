# ANTIGRAVITY.md

Guia para Antigravity en `Neros`. Usar esta guia como puente hacia `AGENTS.md`, `CONVENTIONS.md`, agents locales y skills.

## Fuentes De Verdad

- Proyecto: `AGENTS.md`.
- Convenciones: `CONVENTIONS.md`.
- Skills: `skills/*/SKILL.md`.
- Agents reutilizables: `.vscode/agents/*.md` y `.cursor/agents/*.md`.
- Reglas Cursor como referencia adicional: `.cursor/rules/*.mdc`.

## Modelo Del Proyecto

- .NET 10.
- Blazor Web App + Tailwind CSS.
- ASP.NET Core Web API.
- Clean Architecture: Blazor, Api, Application, Domain, Contracts, Shared.
- SQL Server con scripts manuales versionados.
- Identity, SignalR y jobs respetando puertos de Application.
- Graphify para arquitectura/impacto cuando se permita ejecutar comandos.

## Roles Equivalentes

| Necesidad | Agent |
|---|---|
| Planificar feature/migracion | `.vscode/agents/planner.md` |
| Implementar vertical completa | `.vscode/agents/dev.md` |
| API/Application/Domain/SQL | `.vscode/agents/backend.md` |
| Blazor/Tailwind/UX | `.vscode/agents/frontend.md` |
| Revision final | `.vscode/agents/qa.md` |

## Reglas Duras

- No ejecutar comandos locales si el usuario lo prohibe.
- No introducir Bootstrap, jQuery, DataTables ni Select2.
- No usar MVC Areas o `Servicios/*` como destino de arquitectura.
- Blazor no consulta base de datos.
- API no contiene reglas de negocio extensas.
- Domain no depende de infraestructura.
- No secretos en archivos, logs, prompts ni respuestas.

## Workflow

1. Clasificar la tarea: plan, dev, backend, frontend, qa, seguridad, datos o diagnostico.
2. Leer el agent equivalente.
3. Cargar skills aplicables.
4. Trabajar por slices pequenos.
5. Validar con diagnosticos del editor o comandos permitidos.
6. Reportar validaciones pendientes si no se ejecuto local.

## Comandos Canonicos

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```