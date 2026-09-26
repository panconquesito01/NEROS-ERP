# Graph Report - Neros  (2026-09-25)

## Corpus Check
- 185 files · ~71,092 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1459 nodes · 1816 edges · 138 communities (115 shown, 23 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 38 edges (avg confidence: 0.76)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `57efeadf`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Neros.Shared
- settings.json
- http
- http
- _Imports.razor
- RedAcceso
- ReconnectModal.razor.js
- App.razor
- AzureDevOps
- Routes.razor
- WeatherForecast.cs
- Error.razor
- Class1.cs
- Class1.cs
- Class1.cs
- Class1.cs
- Counter.razor
- Weather.razor
- NavMenu
- NavLink
- Home.razor
- C# Async â€” Neros
- Program.cs
- ReconnectModal.razor
- NotFound.razor
- CONVENTIONS.md
- package.json
- copilot-instructions.md
- Authoring Instructions Files â€” Neros
- Backend Agent
- Frontend Agent
- Planner Agent
- QA Agent
- Crear Endpoint API â€” Neros
- Crear Feature Blazor â€” Neros
- What the Output Must Contain
- ASP.NET Core MVC + Razor â€” Neros
- Common Neros Code Smells & Fixes
- Crear Caso De Uso â€” Neros
- Crear Script SQL â€” Neros
- Desarrollar Reempaque y Saldos Pendientes â€” Neros
- README.md
- .NET Best Practices â€” Neros
- Review Checklist
- AGENTS.md
- EF Core â€” Neros
- Workflow
- backend.md
- frontend.md
- javascript-frontend.instructions.md
- Authoring Agents â€” Neros
- code-review.chatmode.md
- razor.instructions.md
- planner.md
- AGENTS.md
- qa.md
- architect.chatmode.md
- C# Async â€” Neros
- markdown.instructions.md
- AGENTS.md
- Neros.Shared.csproj
- Diagnose - Neros
- GitHub Actions â€” Neros
- Create Implementation Plan â€” Neros
- NerosDbContext
- Architecture Blueprint Generator - Neros
- Acquire Codebase Knowledge - Neros
- GEMINI.md
- planner-fast.chatmode.md
- Authoring Agent Skills â€” Neros
- FastReport â€” Neros
- FabricaApi
- debug.chatmode.md
- refactor.chatmode.md
- ASP.NET MVC/Razor Legacy Reference - Neros
- Dev Agent
- LoginAccessPanel.razor
- Dev Agent
- CODEX.md
- Empresas.razor
- ANTIGRAVITY.md
- Neros.Blazor.Components.Features.Auth
- Login.razor
- CLAUDE.md
- C# - Neros
- EF Core - Neros
- Antigravity Bridge - Neros
- Codex Bridge - Neros
- Security - Neros
- Antigravity Agents - Neros
- Codex Agents - Neros
- LoginProductStage.razor
- experience.js
- theme-init.js
- AuthLayout.razor
- SelectorTema.razor
- HttpGet
- string
- ReconnectModal.razor
- LoginAmbient.razor
- LoginHeader.razor
- LoginModuleTile.razor
- Icono.razor
- Neros ERP - Estrategia Gerencial, Comercial y de Producto
- Base multiempresa de Neros ERP
- Configuracion por entorno y secretos de Neros
- Validacion de acceso multiempresa
- README.md
- Validacion de acceso multiempresa
- Base multiempresa de Neros ERP
- Configuracion por entorno y secretos de Neros
- ADR-0002: Distributed Modular ERP Platform
- ADR-0001: evolucion incremental a monolito modular
- Base de datos Neros ERP
- FoundationHttpTests.cs
- ServiceAuthentication.cs
- .SendAsync
- _Imports.razor
- .TryFromAuthenticatedPrincipal
- Neros.ServiceDefaults
- NEROS: estado de ejecucion
- ServicioIdentidad
- SecurityContext
- EvidenceExporter
- AutenticacionSesion
- D-01: contratos, amenazas y transicion
- .EjecutarAsync
- IEnumerable
- EmpresaDisponible
- EstadoVacio.razor
- System.Security.Claims
- CONVENTIONS.md
- SelectorTema.razor

## God Nodes (most connected - your core abstractions)
1. `NEROS ERP: arquitectura objetivo distribuida` - 18 edges
2. `NEROS ERP: diagnostico de arquitectura` - 16 edges
3. `AccesoMultiempresaTests` - 15 edges
4. `Neros ERP - Estrategia Gerencial, Comercial y de Producto` - 15 edges
5. `.NET Best Practices - Neros` - 14 edges
6. `System.Security.Claims` - 13 edges
7. `ClienteNeros` - 13 edges
8. `EmpresaDisponible` - 13 edges
9. `Neros.ServiceDefaults` - 13 edges
10. `NEROS ERP: roadmap de plataforma distribuida` - 13 edges

## Surprising Connections (you probably didn't know these)
- `FabricaApi` --references--> `AccesoController`  [EXTRACTED]
  tests/Neros.Tests/EntornoPruebas.cs → Neros.Api/Controllers/AccesoController.cs
- `RepositorioEmpresas` --implements--> `IRepositorioEmpresas`  [EXTRACTED]
  Neros.Persistence/Seguridad/RepositorioEmpresas.cs → Neros.Application/Autenticacion/IServicioIdentidad.cs
- `ServicioIdentidad` --implements--> `IServicioIdentidad`  [EXTRACTED]
  Neros.Persistence/Seguridad/ServicioIdentidad.cs → Neros.Application/Autenticacion/IServicioIdentidad.cs
- `ServicioIdentidad` --references--> `Usuario`  [EXTRACTED]
  Neros.Persistence/Seguridad/ServicioIdentidad.cs → Neros.Persistence/Seguridad/ModeloSeguridad.cs
- `NerosDbContext` --references--> `Empresa`  [EXTRACTED]
  Neros.Persistence/NerosDbContext.cs → Neros.Persistence/Seguridad/ModeloSeguridad.cs

## Import Cycles
- None detected.

## Communities (138 total, 23 thin omitted)

### Community 0 - "Neros.Shared"
Cohesion: 0.06
Nodes (38): net10.0, Microsoft.NET.Sdk.Web, Neros.Application, net10.0, Microsoft.NET.Sdk.Web, Neros.Contracts, Neros.Domain, Neros.Persistence (+30 more)

### Community 1 - "settings.json"
Cohesion: 0.12
Nodes (16): agentsPath, buildCommand, _comment, _comment_hooks, defaultLanguage, enabledPlugins, feature-dev@claude-plugins-official, frontend-design@claude-plugins-official (+8 more)

### Community 2 - "http"
Cohesion: 0.08
Nodes (25): ClusterConfig, Neros.Organization.Api, RouteConfig, SecurityKey, WebApplication, WebApplicationBuilder, GatewayHost, string (+17 more)

### Community 3 - "http"
Cohesion: 0.05
Nodes (36): Actividad y refresco, Botones y controles de icono, Campos y acceso, Catalogo comercial del acceso, Cierre ship y limites de verificacion, Colors, Components, Do: (+28 more)

### Community 4 - "_Imports.razor"
Cohesion: 0.20
Nodes (8): Neros.Api.Seguridad, Neros.Api.Controllers, Neros.Blazor.Servicios, Neros.Application.Autenticacion, Neros.Contracts.Autenticacion, Microsoft.AspNetCore.Authorization, Neros.Blazor.Components, System.Security.Claims

### Community 6 - "ReconnectModal.razor.js"
Cohesion: 0.32
Nodes (6): handleReconnectStateChanged(), reconnectModal, resumeButton, retry(), retryButton, retryWhenDocumentBecomesVisible()

### Community 7 - "App.razor"
Cohesion: 0.33
Nodes (5): HeadOutlet, ImportMap, ReconnectModal, ResourcePreloader, Routes

### Community 8 - "AzureDevOps"
Cohesion: 0.33
Nodes (5): AzureDevOps, postman, ADO_MCP_AUTH_TOKEN, npx, @azure-devops/mcp

### Community 9 - "Routes.razor"
Cohesion: 0.20
Nodes (9): AuthorizeRouteView, FocusOnNavigate, Found, Acciones, ChildContent, EstadoVacio, Icono, NotAuthorized (+1 more)

### Community 10 - "WeatherForecast.cs"
Cohesion: 0.50
Nodes (3): WeatherForecast, Neros.Api, DateOnly

### Community 11 - "Error.razor"
Cohesion: 0.06
Nodes (29): DelegatingHandler, HealthReport, IApplicationBuilder, IExceptionHandler, IHostApplicationBuilder, IHttpClientBuilder, object, CancellationToken (+21 more)

### Community 16 - "Counter.razor"
Cohesion: 0.14
Nodes (14): Acceso e inicio multiempresa, Alcance, Alta inicial, Arquitectura, Configuracion local, Contrato HTTP, Ejecucion, Interfaz (+6 more)

### Community 17 - "Weather.razor"
Cohesion: 0.11
Nodes (23): App, IAsyncLifetime, IClassFixture, IHost, IHostBuilder, IPage, IWebHostBuilder, AccesoConcedido (+15 more)

### Community 18 - "NavMenu"
Cohesion: 0.22
Nodes (8): NavMenu, AntiforgeryToken, Icono, IHttpContextAccessor, LayoutComponentBase, Marca, NavigationManager, SelectorTema

### Community 19 - "NavLink"
Cohesion: 0.40
Nodes (4): NavLink, Icono, IHttpContextAccessor, Marca

### Community 20 - "Home.razor"
Cohesion: 0.17
Nodes (11): ActividadReciente, ContextoEmpresa, ModulosEnPreparacion, OnInitializedAsync, Aviso, ClienteNeros, Icono, IHttpContextAccessor (+3 more)

### Community 21 - "C# Async â€” Neros"
Cohesion: 0.22
Nodes (8): Banned, By Layer, C# Async - Neros, Cancellation, Exceptions, Names, Parallelism, Return Types

### Community 22 - "Program.cs"
Cohesion: 0.15
Nodes (16): HttpMethod, IAntiforgery, IResult, CancellationToken, Guid, HttpRequestMessage, IReadOnlyList, Task (+8 more)

### Community 23 - "ReconnectModal.razor"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 24 - "NotFound.razor"
Cohesion: 0.15
Nodes (7): Neros.Blazor.Components.Shared, IEnumerable, IReadOnlyList, CatalogoModulos, ModuloErp, Presentacion, TipoAviso

### Community 25 - "CONVENTIONS.md"
Cohesion: 0.09
Nodes (20): Neros.Messaging.Abstractions, DateTimeOffset, Guid, JsonElement, IntegrationEnvelope, CancellationToken, Guid, Task (+12 more)

### Community 26 - "package.json"
Cohesion: 0.15
Nodes (12): lucide-static, dependencies, lucide-static, devDependencies, tailwindcss, name, private, scripts (+4 more)

### Community 27 - "copilot-instructions.md"
Cohesion: 0.18
Nodes (8): ADR-0003: foundation de desarrollo y contratos de borde, Alternativas y consecuencias, Decisiones, Verificacion y limites, D-02: Gateway y host Organization, Ejecucion local, Evidencia y limites, Implementacion

### Community 28 - "Authoring Instructions Files â€” Neros"
Cohesion: 0.22
Nodes (8): Authoring Instructions Files - Neros, Body Structure, File Naming, Front Matter, Good applyTo Examples, Neros Content Rules, Validation Checklist, When To Create A New File

### Community 29 - "Backend Agent"
Cohesion: 0.20
Nodes (9): Backend Agent, Checklist de diseno, Common Backend Findings To Prevent, Output Contract, Proposito, Reglas, Skills To Load, Validacion (+1 more)

### Community 30 - "Frontend Agent"
Cohesion: 0.20
Nodes (9): Checklist UI, Frontend Agent, Output Contract, Proposito, Reglas, Skills To Load, UI Anti-Patterns, Validacion (+1 more)

### Community 31 - "Planner Agent"
Cohesion: 0.22
Nodes (8): Handoff Rules, Mission, Output Contract, Planner Agent, Required Context, Skills To Load, Stop Conditions, Workflow

### Community 32 - "QA Agent"
Cohesion: 0.20
Nodes (9): Checklist, Comandos, Mission, No Findings Output, QA Agent, Review Procedure, Salida, Severity Rubric (+1 more)

### Community 33 - "Crear Endpoint API â€” Neros"
Cohesion: 0.40
Nodes (4): Crear Endpoint API - Neros, Pasos, Reglas, Validacion

### Community 34 - "Crear Feature Blazor â€” Neros"
Cohesion: 0.40
Nodes (4): Crear Feature Blazor - Neros, Pasos, Reglas, Validacion

### Community 35 - "What the Output Must Contain"
Cohesion: 0.29
Nodes (6): Client, Groups, Payloads, Server, SignalR - Neros, Validation

### Community 36 - "ASP.NET Core MVC + Razor â€” Neros"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 37 - "Common Neros Code Smells & Fixes"
Cohesion: 0.14
Nodes (13): Checks, Common Refactor Smells, Do Not, Extract Application Use Case, Output, Principle, Refactor - Neros, Refactor Recipes (+5 more)

### Community 38 - "Crear Caso De Uso â€” Neros"
Cohesion: 0.40
Nodes (4): Crear Caso De Uso - Neros, Pasos, Reglas, Validacion

### Community 39 - "Crear Script SQL â€” Neros"
Cohesion: 0.33
Nodes (5): Contenido esperado, Crear Script SQL - Neros, Nombre sugerido, Reglas, Validacion

### Community 41 - "Desarrollar Reempaque y Saldos Pendientes â€” Neros"
Cohesion: 0.18
Nodes (10): Arquitectura obligatoria, Diseno por capas, Entrega, No hacer, Objetivo funcional, Preflight, Reempaque Y Saldos Pendientes - Neros, Reglas de negocio (+2 more)

### Community 42 - "README.md"
Cohesion: 0.12
Nodes (13): Constraints, Daily Repo Status Report, Inputs To Fetch, Output, Agentic Workflows - Neros, File Format, Files, Rules (+5 more)

### Community 43 - ".NET Best Practices â€” Neros"
Cohesion: 0.13
Nodes (14): API Response Pattern, Architecture, Common Neros Code Smells And Fixes, Configuration, Data, Dependency Injection, DI Checklist, Errors (+6 more)

### Community 44 - "Review Checklist"
Cohesion: 0.25
Nodes (7): Avoid, Common Review Findings In Neros, Output Format, Review Checklist, Review Procedure, Reviewing Code - Neros, Severity Rubric

### Community 45 - "AGENTS.md"
Cohesion: 0.50
Nodes (3): Proyecto, Puentes Compatibles, Reglas

### Community 46 - "EF Core â€” Neros"
Cohesion: 0.17
Nodes (11): Common Mistakes And Fixes, EF Core / SQL Server - Neros, Manual SQL Script Template, Performance Checklist, Placement, Query Examples, Query Rules, Schema Changes (+3 more)

### Community 47 - "Workflow"
Cohesion: 0.18
Nodes (10): Checklist, Escalate To, Identity And Authorization Checks, Neros Security Model, Neros Threat Patterns, Output, Review Workflow, Scope (+2 more)

### Community 48 - "backend.md"
Cohesion: 0.20
Nodes (9): Backend Agent, Checklist de diseno, Common Backend Findings To Prevent, Output Contract, Proposito, Reglas, Skills To Load, Validacion (+1 more)

### Community 49 - "frontend.md"
Cohesion: 0.20
Nodes (9): Checklist UI, Frontend Agent, Output Contract, Proposito, Reglas, Skills To Load, UI Anti-Patterns, Validacion (+1 more)

### Community 51 - "Authoring Agents â€” Neros"
Cohesion: 0.22
Nodes (8): Authoring Agents - Neros, File Locations, Front Matter, Neros Defaults, Required Sections, Skills, Tooling Rules, Validation Checklist

### Community 52 - "code-review.chatmode.md"
Cohesion: 0.50
Nodes (3): Code Review Mode - Neros, Output, Priorities

### Community 53 - "razor.instructions.md"
Cohesion: 0.40
Nodes (4): After Editing, Blazor - Neros, Forms, UX Rules

### Community 54 - "planner.md"
Cohesion: 0.22
Nodes (8): Handoff Rules, Mission, Output Contract, Planner Agent, Required Context, Skills To Load, Stop Conditions, Workflow

### Community 55 - "AGENTS.md"
Cohesion: 0.13
Nodes (13): Comandos canonicos, Direccion de dependencias, Estructura, Graphify, Proyecto, Puentes De Agente, Red flags que deben detener una implementacion, Reglas duras (+5 more)

### Community 56 - "qa.md"
Cohesion: 0.20
Nodes (9): Checklist, Comandos, Mission, No Findings Output, QA Agent, Review Procedure, Salida, Severity Rubric (+1 more)

### Community 57 - "architect.chatmode.md"
Cohesion: 0.50
Nodes (3): Architect Mode - Neros, Output, Workflow

### Community 58 - "C# Async â€” Neros"
Cohesion: 0.18
Nodes (10): By Layer, C# Async - Neros, Common Mistakes And Fixes, ConfigureAwait, Defaults, Escalate To, Examples, Parallelism (+2 more)

### Community 60 - "AGENTS.md"
Cohesion: 0.50
Nodes (3): Flujo recomendado, Puentes Compatibles, Reglas compartidas

### Community 62 - "Neros.Shared.csproj"
Cohesion: 0.20
Nodes (8): net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk

### Community 63 - "Diagnose - Neros"
Cohesion: 0.13
Nodes (14): API Endpoint Fails, Blazor Page Fails, Common Failure Surfaces, Diagnose - Neros, Diagnostic Playbooks, Error Pattern Table, If Local Commands Are Allowed, If Local Commands Are Not Allowed (+6 more)

### Community 64 - "GitHub Actions â€” Neros"
Cohesion: 0.29
Nodes (6): Agentic Workflows, Caching, Do Not, GitHub Actions - Neros, Neros Build Commands, Standard YAML Workflows

### Community 65 - "Create Implementation Plan â€” Neros"
Cohesion: 0.22
Nodes (8): Acceptance Criteria, Common Plan Gaps To Avoid, Create Implementation Plan - Neros, Output, Plan Template, Planning Heuristics, Preflight, Rules

### Community 66 - "NerosDbContext"
Cohesion: 0.16
Nodes (18): DbSet, IdentityUser, IdentityUserContext, IQueryable, ModelBuilder, NerosDbContext, DateTimeOffset, Guid (+10 more)

### Community 67 - "Architecture Blueprint Generator - Neros"
Cohesion: 0.14
Nodes (13): Architecture Blueprint Generator - Neros, Blueprint Sections, Common Blueprint Mistakes, Data View, Deployment/Operations View, Inputs, Legacy Boundary, Output Quality (+5 more)

### Community 68 - "Acquire Codebase Knowledge - Neros"
Cohesion: 0.15
Nodes (12): Acquire Codebase Knowledge - Neros, Architecture Map, Common Discovery Mistakes, Escalate To, Evidence To Prefer, Exploration Workflow, Goal, Layer Ownership Guide (+4 more)

### Community 69 - "GEMINI.md"
Cohesion: 0.40
Nodes (4): Comandos clave, No hacer, Prioridad, Proyecto

### Community 71 - "Authoring Agent Skills â€” Neros"
Cohesion: 0.22
Nodes (8): Anti-patterns, Authoring Agent Skills - Neros, Body Structure, Commands To Reference, Folder Layout, Neros Defaults, Required Front Matter, Validation Checklist

### Community 72 - "FastReport â€” Neros"
Cohesion: 0.33
Nodes (5): Data, Output, Placement, Reports And PDF - Neros, Rules

### Community 73 - "FabricaApi"
Cohesion: 0.09
Nodes (22): Neros.Organization.Migration, IReadOnlyCollection, List, Fact, Task, OrganizationMigrationTests, Guid, IEnumerable (+14 more)

### Community 74 - "debug.chatmode.md"
Cohesion: 0.50
Nodes (3): Debug Mode - Neros, Output, Workflow

### Community 75 - "refactor.chatmode.md"
Cohesion: 0.50
Nodes (3): Output, Refactor Mode - Neros, Rules

### Community 76 - "ASP.NET MVC/Razor Legacy Reference - Neros"
Cohesion: 0.17
Nodes (11): ASP.NET MVC/Razor Legacy Reference - Neros, Common Migration Traps, Do Not, Escalate To, Legacy Extraction Workflow, Migration Pattern, Output, Target For New Work (+3 more)

### Community 77 - "Dev Agent"
Cohesion: 0.20
Nodes (9): Anti-Patterns, Dev Agent, Entrega esperada, Mision, Reglas de implementacion, Required Context, Skills To Load, Validacion (+1 more)

### Community 78 - "LoginAccessPanel.razor"
Cohesion: 0.33
Nodes (5): Detalle, OnInitialized, ChildContent, PageTitle, PaginaEstado

### Community 79 - "Dev Agent"
Cohesion: 0.20
Nodes (9): Anti-Patterns, Dev Agent, Entrega esperada, Mision, Reglas de implementacion, Required Context, Skills To Load, Validacion (+1 more)

### Community 80 - "CODEX.md"
Cohesion: 0.22
Nodes (7): Arquitectura Objetivo, Comandos Canonicos, Entrega, Flujo Recomendado, Prioridad, Reglas Duras, Seleccion De Skill

### Community 81 - "Empresas.razor"
Cohesion: 0.15
Nodes (12): FilaEmpresa, OnInitializedAsync, Acciones, Aviso, ChildContent, ClienteNeros, EmpresaDisponible, EstadoVacio (+4 more)

### Community 82 - "ANTIGRAVITY.md"
Cohesion: 0.25
Nodes (6): Comandos Canonicos, Fuentes De Verdad, Modelo Del Proyecto, Reglas Duras, Roles Equivalentes, Workflow

### Community 83 - "Neros.Blazor.Components.Features.Auth"
Cohesion: 0.50
Nodes (3): ActividadAcceso, EstadoVacio, Icono

### Community 84 - "Login.razor"
Cohesion: 0.22
Nodes (8): CatalogoAcceso, AntiforgeryToken, Icono, Marca, PageTitle, SelectorTema, route:/, route:/login

### Community 85 - "CLAUDE.md"
Cohesion: 0.33
Nodes (5): Arquitectura objetivo, Comandos canonicos, Contexto, Graphify, Reglas de trabajo

### Community 86 - "C# - Neros"
Cohesion: 0.33
Nodes (5): After Editing, C# - Neros, Dependency Injection, Layers, Security And Data

### Community 87 - "EF Core - Neros"
Cohesion: 0.33
Nodes (5): EF Core - Neros, Placement, Queries, Schema, Transactions

### Community 88 - "Antigravity Bridge - Neros"
Cohesion: 0.40
Nodes (4): Agents, Antigravity Bridge - Neros, Reglas, Skills

### Community 89 - "Codex Bridge - Neros"
Cohesion: 0.40
Nodes (4): Agents, Codex Bridge - Neros, Reglas, Skills

### Community 90 - "Security - Neros"
Cohesion: 0.40
Nodes (4): Authorization, Input And Output, Review Triggers, Security - Neros

### Community 91 - "Antigravity Agents - Neros"
Cohesion: 0.50
Nodes (3): Antigravity Agents - Neros, Flujo, Reglas Compartidas

### Community 92 - "Codex Agents - Neros"
Cohesion: 0.50
Nodes (3): Codex Agents - Neros, Flujo, Fuente De Verdad

### Community 93 - "LoginProductStage.razor"
Cohesion: 0.50
Nodes (3): Icono, Marca, SelectorTema

### Community 105 - "Neros ERP - Estrategia Gerencial, Comercial y de Producto"
Cohesion: 0.08
Nodes (25): 10. Comercializacion y aliados, 11. Futura web comercial independiente, 12. Materiales y gobierno comercial, 13. Condiciones para avanzar, 1. Decision ejecutiva, 2. Que es Neros, 3. Estado real del producto, 4. Catalogo comercial propuesto (+17 more)

### Community 106 - "Base multiempresa de Neros ERP"
Cohesion: 0.09
Nodes (23): 10. Refactorizaciones necesarias, 11. Roadmap tecnico, 12. Roadmap funcional, 13. Dependencias funcionales, 14. Riesgos y bloqueos, 1. Estado actual, 2. Modulos existentes, 3. Arquitectura encontrada (+15 more)

### Community 107 - "Configuracion por entorno y secretos de Neros"
Cohesion: 0.11
Nodes (18): 10. NEROS Jobs y archivos, 11. Configuracion, contenedores, discovery y CI/CD, 12. Observabilidad distribuida, 13. Data Platform, Search y localizaciones, 14. Escalabilidad y prueba de autonomia, 15. Estructura .NET propuesta y reutilizacion, 16. Transicion sin ruptura, 17. Gobierno y entregables (+10 more)

### Community 109 - "README.md"
Cohesion: 0.12
Nodes (17): 10. Decisiones y lista de aplazamiento, 11. Trazabilidad del roadmap anterior, 12. I-01: cierre historico conservado, 1. Orden y fundamento en el repositorio, 2. Top 10 y reglas de prioridad, 3. Foundation y Platform Infrastructure: ejecucion exacta, 4. Identity / Organization: ownership y corte, 5. Master Data y primer vertical real (+9 more)

### Community 110 - "Validacion de acceso multiempresa"
Cohesion: 0.22
Nodes (9): Casos cubiertos, Confirmacion de autenticacion y arranque, Errores de acceso sin recarga, Observaciones, Red animada y catalogo comercial, Resultado, Revision de acabado, Revision del login y estrategia comercial (+1 more)

### Community 111 - "Base multiempresa de Neros ERP"
Cohesion: 0.25
Nodes (8): Base multiempresa de Neros ERP, Criterio de aceptacion, Decisiones gerenciales necesarias, Lo que no incluye, Puesta en marcha, Resumen ejecutivo, Siguiente etapa, Valor entregado

### Community 112 - "Configuracion por entorno y secretos de Neros"
Cohesion: 0.25
Nodes (8): Como obtiene la conexion, Configuracion por entorno y secretos de Neros, Desarrollo y produccion, Efecto de Gitignore, Fuentes, Incorporar otro desarrollador, Resultado de la revision, Si aparece un secreto en Git

### Community 113 - "ADR-0002: Distributed Modular ERP Platform"
Cohesion: 0.29
Nodes (7): ADR-0002: Distributed Modular ERP Platform, Alternativas, Consecuencias y costos aceptados, Contexto y evidencia, Decision, Decisiones acotadas pendientes, Transicion y criterios de aceptacion

### Community 114 - "ADR-0001: evolucion incremental a monolito modular"
Cohesion: 0.33
Nodes (6): ADR-0001: evolucion incremental a monolito modular, Alternativas, Compatibilidad y verificacion, Consecuencias, Contexto, Decision

### Community 115 - "Base de datos Neros ERP"
Cohesion: 0.50
Nodes (4): Base de datos Neros ERP, Instalacion, Reversion, Seguridad y operacion

### Community 116 - "FoundationHttpTests.cs"
Cohesion: 0.11
Nodes (15): ConcurrentQueue, EventId, Func, IDisposable, ILoggerProvider, LogLevel, Exception, Fact (+7 more)

### Community 117 - "ServiceAuthentication.cs"
Cohesion: 0.22
Nodes (6): AuthorizationPolicyBuilder, IAuthorizationRequirement, IServiceCollection, Uri, ServiceAuthentication, ServicePermission

### Community 118 - ".SendAsync"
Cohesion: 0.17
Nodes (11): ASPNETCORE_ENVIRONMENT, Identity__Authority, Services__Compatibility, Services__Organization, applicationUrl, commandName, environmentVariables, launchBrowser (+3 more)

### Community 119 - "_Imports.razor"
Cohesion: 0.15
Nodes (12): Microsoft.AspNetCore.Components.Authorization, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.Web.Virtualization, Microsoft.JSInterop, Neros.Blazor, Neros.Blazor.Components.Layout (+4 more)

### Community 120 - ".TryFromAuthenticatedPrincipal"
Cohesion: 0.10
Nodes (20): AuthorizationHandler, AuthorizationHandlerContext, Claim, IReadOnlySet, ServicePermission, ClaimsPrincipal, DateTimeOffset, Guid (+12 more)

### Community 121 - "Neros.ServiceDefaults"
Cohesion: 0.19
Nodes (9): Neros.ServiceDefaults, Neros.Tests, HttpMessageHandler, int, System.Diagnostics, FoundationAuthenticationTests, Request, SpanEvidence (+1 more)

### Community 122 - "NEROS: estado de ejecucion"
Cohesion: 0.22
Nodes (9): Completed gates, Current blockers, Current initiative, Important decisions, Migrations, NEROS: estado de ejecucion, Next action, Pending gates (+1 more)

### Community 123 - "ServicioIdentidad"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, Identity__Authority, applicationUrl, commandName, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 124 - "SecurityContext"
Cohesion: 0.06
Nodes (40): AllowAnonymous, WeatherForecastController, ControllerBase, EnableRateLimiting, IActionResult, ActionResult, CancellationToken, HttpGet (+32 more)

### Community 125 - "EvidenceExporter"
Cohesion: 0.40
Nodes (5): Activity, BaseExporter, Batch, ExportResult, EvidenceExporter

### Community 126 - "AutenticacionSesion"
Cohesion: 0.18
Nodes (9): AuthenticationHandler, AuthenticateResult, AuthenticationSchemeOptions, Task, AutenticacionSesion, AuthenticateResult, AuthenticationSchemeOptions, Task (+1 more)

### Community 127 - "D-01: contratos, amenazas y transicion"
Cohesion: 0.33
Nodes (6): Contratos de borde, D-01: contratos, amenazas y transicion, Gate D-01, Mensajeria e idempotencia, Migracion y autorizacion, Threat model y evidencia

### Community 128 - ".EjecutarAsync"
Cohesion: 0.47
Nodes (3): IServiceProvider, Task, ComandosAdministracion

### Community 130 - "EmpresaDisponible"
Cohesion: 0.33
Nodes (3): Neros.Gateway, Task, Program

### Community 132 - "System.Security.Claims"
Cohesion: 0.33
Nodes (4): Neros.Persistence.Seguridad, Neros.Persistence, Program, System.Net.Http.Json

### Community 133 - "CONVENTIONS.md"
Cohesion: 0.17
Nodes (10): API Y Application, Base De Datos, Capas, Dependencias Permitidas, Estructura De Neros.Blazor, Frontend, Idioma Y Nombres, No Hacer (+2 more)

## Knowledge Gaps
- **726 isolated node(s):** `$schema`, `frontend-design@claude-plugins-official`, `feature-dev@claude-plugins-official`, `_comment`, `skillsPath` (+721 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **23 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Neros.Tests` connect `Neros.ServiceDefaults` to `FoundationHttpTests.cs`, `FabricaApi`, `System.Security.Claims`?**
  _High betweenness centrality (0.036) - this node is a cross-community bridge._
- **Why does `Neros.ServiceDefaults` connect `Neros.ServiceDefaults` to `EmpresaDisponible`, `http`, `System.Security.Claims`, `_Imports.razor`, `Error.razor`, `FoundationHttpTests.cs`, `ServiceAuthentication.cs`, `.TryFromAuthenticatedPrincipal`?**
  _High betweenness centrality (0.023) - this node is a cross-community bridge._
- **Why does `System.Security.Claims` connect `_Imports.razor` to `.TryFromAuthenticatedPrincipal`, `Neros.ServiceDefaults`, `System.Security.Claims`, `_Imports.razor`?**
  _High betweenness centrality (0.022) - this node is a cross-community bridge._
- **What connects `$schema`, `frontend-design@claude-plugins-official`, `feature-dev@claude-plugins-official` to the rest of the system?**
  _726 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Neros.Shared` be split into smaller, more focused modules?**
  _Cohesion score 0.058279370952821465 - nodes in this community are weakly interconnected._
- **Should `settings.json` be split into smaller, more focused modules?**
  _Cohesion score 0.11764705882352941 - nodes in this community are weakly interconnected._
- **Should `http` be split into smaller, more focused modules?**
  _Cohesion score 0.0824524312896406 - nodes in this community are weakly interconnected._