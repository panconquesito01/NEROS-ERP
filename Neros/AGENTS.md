# AGENTS.md

Guia operativa principal para agentes de IA que trabajen en `Neros`.

## Proyecto

- Producto: Distributed Modular ERP Platform SaaS, full .NET, con frontend Blazor. Decision vigente: [ADR-0002](docs/adr/ADR-0002-distributed-modular-platform.md).
- Stack objetivo: .NET 10, Blazor Web App, ASP.NET Core Web API, Clean Architecture, Tailwind CSS, SQL Server, Identity, SignalR, jobs y Graphify.
- Solucion: `Neros.slnx`.
- Idioma: codigo, UI y documentacion funcional en espanol, usando nombres de negocio claros.
- El sistema anterior es referencia funcional de migracion, no arquitectura objetivo.

## Estructura

El arbol siguiente describe la compatibilidad existente, no un nucleo empresarial compartido para servicios nuevos. Seguir [roadmap D-01..D-18](docs/NEROS_ERP_IMPLEMENTATION_ROADMAP.md) y [estado de ejecucion](docs/execution/NEROS_EXECUTION_STATUS.md); no crear proyectos vacios ni saltar gates.

```text
Neros/
|-- Neros.Shared/        # Tipos transversales simples
|-- Neros.Domain/        # Entidades, value objects, reglas puras
|-- Neros.Contracts/     # DTOs, requests, responses y contratos publicos
|-- Neros.Application/   # Casos de uso, puertos, validaciones y reglas de aplicacion
|-- Neros.Blazor/        # Frontend Blazor Web App + Tailwind CSS
|-- Neros.Api/           # API HTTP para Blazor e integraciones
|-- skills/              # Skills locales para agentes
`-- graphify-out/        # Grafo generado por Graphify
```

## Puentes De Agente

- Copilot: `.github/copilot-instructions.md`, `.github/chatmodes/`, `.github/prompts/`.
- VS Code agents: `.vscode/AGENTS.md` y `.vscode/agents/*.md`.
- Cursor agents: `.cursor/AGENTS.md`, `.cursor/agents/*.md`, `.cursor/rules/*.mdc`.
- Claude: `CLAUDE.md` y `.claude/README.md`.
- Gemini: `GEMINI.md`.
- Codex: `CODEX.md` y `.codex/`.
- Antigravity: `ANTIGRAVITY.md` y `.antigravity/`.

Todos deben converger en esta guia, `CONVENTIONS.md` y `skills/*/SKILL.md`.

## Direccion de dependencias

En cada bounded context nuevo: Api -> Application -> Domain; Infrastructure implementa puertos locales, Contracts contiene DTOs/eventos del propietario. Base, DbContext, scripts y despliegue independientes. Prohibidos SQL/FK/DbContext entre servicios y referencias a Domain/Application/Persistence ajenos. Integrar por HTTP/eventos/proyecciones autorizadas. BuildingBlocks solo tecnicos. La direccion siguiente aplica a los hosts de compatibilidad hasta su extraccion.

```text
Neros.Blazor -> Neros.Contracts / Neros.Shared
Neros.Blazor -> Neros.Api via HTTP o servicios cliente tipados
Neros.Api -> Neros.Application
Neros.Application -> Neros.Domain / Neros.Contracts / Neros.Shared
Neros.Domain -> sin dependencias de infraestructura
Infraestructura/Persistence futura -> implementa puertos de Application
```

## Reglas duras

- Tenant -> BusinessGroup -> Company -> Branch. Cada empresa pertenece a un unico tenant contractual; sin DefaultTenant ni CompanyId como TenantId. TenantId obligatorio en datos nuevos de negocio y eventos. Mapeo de migracion explicito; correspondencia no resuelta se rechaza.
- AdministradorGlobal tiene acceso transversal por decision del usuario: privilegio de plataforma explicito, tenant seleccionado, autorizacion y auditoria server-side. No equivale a eliminar filtros tenant o confiar en un header/rol no validado. Permisos empresariales normales se resuelven por tenant.
- Revocacion vigente y fail closed en operaciones sensibles cuando Identity/Organization no puedan confirmar autorizacion. No relajar SLA mediante caches sin decision explicita.
- Shared/sharded/dedicated usan los mismos contratos/codigo, sin forks. Routing de tenant no cambia ownership funcional.

- No usar el nombre `Neros.Next`; el proyecto se llama `Neros`.
- No reintroducir Bootstrap, jQuery, DataTables ni Select2 en `Neros.Blazor`.
- Usar Tailwind CSS para toda UI nueva.
- `Neros.Blazor` no debe referenciar persistencia ni usar DbContext directamente.
- `Neros.Api` orquesta endpoints, autenticacion/autorizacion y delega en Application.
- `Neros.Application` define casos de uso, puertos, validaciones, transacciones y politicas; no implementa SQL, HTTP externo ni filesystem.
- `Neros.Domain` no referencia infraestructura, EF Core, ASP.NET, Blazor ni paquetes externos de UI.
- `Neros.Contracts` contiene DTOs y contratos entre API/Blazor; no debe filtrar entidades EF ni objetos de UI.
- No guardar secretos en repositorio. Usar User Secrets, variables de entorno o secret manager.
- Cambios de base de datos: preferir scripts SQL Server manuales versionados en `database/scripts/`.
- No versionar `.vs/`, `bin/`, `obj/`, `node_modules/` ni caches de Graphify.

## Workflow recomendado

1. Identificar la capa propietaria del cambio.
2. Si hay duda de arquitectura, estructura o impacto, consultar Graphify primero.
3. Hacer el cambio mas pequeno que cierre el flujo funcional.
4. Mantener contratos claros entre UI, API y Application.
5. Validar con el comando mas estrecho disponible cuando el usuario permita ejecucion local.
6. Si no se puede ejecutar local, usar diagnosticos del editor y dejar comandos de verificacion al usuario.

## Comandos canonicos

```powershell
cd .\Neros
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
graphify cluster-only . --no-viz
graphify tree --graph graphify-out\graph.json --output graphify-out\GRAPH_TREE.html --label "Neros"
```

## Graphify

- Antes de responder preguntas de arquitectura, estructura, dependencias o relaciones entre proyectos, usar `graphify query "<pregunta>"` si el usuario permite comandos.
- Si el usuario prohibe ejecutar local, leer `graphify-out/GRAPH_REPORT.md` y archivos cercanos como sustituto estatico.
- Despues de cambios de codigo o documentacion estructural, actualizar el grafo cuando se permita ejecutar comandos.

## Validacion minima

- Cambios `.cs`, `.razor`, `.csproj`: `dotnet build .\Neros.slnx -v minimal`.
- Cambios Tailwind: `npm run css:build` y luego build .NET.
- Cambios de agentes/skills/instrucciones: verificar frontmatter, diagnosticos del editor y buscar terminos heredados.

## Red flags que deben detener una implementacion

- Blazor consultando base de datos o usando DbContext.
- Domain dependiendo de EF Core, ASP.NET, UI o infraestructura.
- API con logica de negocio extensa en controladores.
- DTOs que exponen entidades internas o secretos.
- SQL concatenado con entrada de usuario.
- UI nueva con Bootstrap/jQuery o rutas demo del template.

