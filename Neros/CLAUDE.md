# CLAUDE.md

Guia para Claude Code en `Neros`. Leer tambien `AGENTS.md` antes de cambios no triviales.

## Contexto

`Neros` es la nueva migracion full .NET de Neros: Blazor Web App para frontend, ASP.NET Core Web API para servicios HTTP y capas Core separadas.

## Arquitectura objetivo

```text
Neros.Blazor -> Neros.Api -> Neros.Application -> Neros.Domain
Neros.Blazor -> Neros.Contracts / Neros.Shared
Infraestructura futura -> implementa puertos definidos por Application
```

## Reglas de trabajo

- Usa Tailwind CSS, no Bootstrap ni jQuery.
- No portes MVC Areas, `Servicios/*` ni patrones del monolito anterior como arquitectura nueva.
- Blazor no accede a base de datos ni a DbContext.
- API valida entrada, aplica autorizacion y delega en Application.
- Application contiene casos de uso, puertos, validaciones y contratos de transaccion.
- Domain permanece puro y testeable.
- DTOs entre UI/API viven en `Neros.Contracts`; tipos transversales simples en `Neros.Shared`.
- SQL Server se evoluciona con scripts manuales versionados cuando haya persistencia.
- Para seguridad, pensar en Identity, roles, permisos por recurso, auditoria y no exposicion de secretos.
- Para tiempo real, SignalR debe usar contratos explicitos y grupos por usuario/tenant/recurso cuando aplique.

## Graphify

- Para arquitectura, estructura o impacto, consultar Graphify si existe `graphify-out/graph.json` y el usuario permite comandos.
- Si no se pueden ejecutar comandos, leer `graphify-out/GRAPH_REPORT.md` y los archivos cercanos.

## Comandos canonicos

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

