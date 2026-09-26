# Graph Report - Neros  (2026-09-25)

## Corpus Check
- 254 files · ~99,872 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2232 nodes · 3344 edges · 188 communities (161 shown, 27 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 116 edges (avg confidence: 0.79)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `2515b7a8`
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
- FoundationHttpTests
- Marca.razor
- DesplegadorBaseDatos
- .CambiarAsync
- .InicioAsync
- CancellationToken
- RepositorioEmpresas
- PlanMigracion.cs
- Neros.Contracts.Autenticacion
- Convenciones SQL de Neros
- EmpresaDisponible
- ServicioIdentidad
- .Create
- System.Text.Json
- Guía de despliegue de base de datos
- FoundationHttpTests
- FoundationMessagingTests
- .LeerEmpresasAsync
- SelectorIdioma.razor
- ADR-0004: plan maestro, esquema por scripts SQL y salto autorizado de gates
- 7. Runner, versionamiento y rollback
- ADR-0003: foundation de desarrollo y contratos de borde
- TextosAcceso.cs
- TextosEmpresas.cs
- TextosInicio.cs
- TextosSistema.cs
- Changelog `compatibilidad`
- 0. Control de cambios
- 4. Scripts SQL como fuente de verdad
- PARTE XI — DEFINITION OF DONE
- Preferencias.razor
- FoundationHttp
- .TryHandleAsync
- .DosHostsHttp_PropaganCorrelacionYExportanMismaTraza
- MenuUsuario.razor
- Idiomas
- ContratosCuenta.cs
- FoundationTelemetry
- .RevocarAsync
- TextosAdministracion.cs
- TextosCuenta.cs
- .InicioAsync
- System.Text.Json
- PaginaAceptacionLegal.razor
- RepositorioEmpresas
- EndpointsSesion.cs
- PaginaCookies.razor
- Base multiempresa de Neros ERP
- Permisos
- TextosPrivacidad.cs
- Changelog `privacidad`
- README.md

## God Nodes (most connected - your core abstractions)
1. `ClienteNeros` - 30 edges
2. `EndpointsSesion` - 24 edges
3. `ContextoCliente` - 22 edges
4. `CuentaSeguridadTests` - 20 edges
5. `DespliegueBaseDatosTests` - 20 edges
6. `DesplegadorBaseDatos` - 20 edges
7. `Neros.Application.Seguridad` - 18 edges
8. `NEROS ERP: arquitectura objetivo distribuida` - 18 edges
9. `System.Security.Claims` - 17 edges
10. `Neros.Contracts.Autenticacion` - 17 edges

## Surprising Connections (you probably didn't know these)
- `ServicioIdentidad` --implements--> `IServicioIdentidad`  [EXTRACTED]
  Neros.Persistence/Seguridad/ServicioIdentidad.cs → Neros.Application/Autenticacion/IServicioIdentidad.cs
- `RepositorioEmpresas` --implements--> `IRepositorioEmpresas`  [EXTRACTED]
  Neros.Persistence/Seguridad/RepositorioEmpresas.cs → Neros.Application/Autenticacion/IServicioIdentidad.cs
- `ServicioCuenta` --implements--> `IServicioCuenta`  [EXTRACTED]
  Neros.Persistence/Seguridad/ServicioCuenta.cs → Neros.Application/Cuenta/IServicioCuenta.cs
- `ServicioAdministracionUsuarios` --implements--> `IServicioAdministracionUsuarios`  [EXTRACTED]
  Neros.Persistence/Seguridad/ServicioAdministracionUsuarios.cs → Neros.Application/Cuenta/IServicioCuenta.cs
- `FabricaApi` --references--> `AccesoController`  [EXTRACTED]
  tests/Neros.Tests/EntornoPruebas.cs → Neros.Api/Controllers/AccesoController.cs

## Import Cycles
- None detected.

## Communities (188 total, 27 thin omitted)

### Community 0 - "Neros.Shared"
Cohesion: 0.05
Nodes (41): net10.0, Microsoft.NET.Sdk.Web, Neros.Application, net10.0, Microsoft.NET.Sdk.Web, Neros.Contracts, Neros.Domain, Neros.Persistence (+33 more)

### Community 1 - "settings.json"
Cohesion: 0.12
Nodes (16): agentsPath, buildCommand, _comment, _comment_hooks, defaultLanguage, enabledPlugins, feature-dev@claude-plugins-official, frontend-design@claude-plugins-official (+8 more)

### Community 2 - "http"
Cohesion: 0.07
Nodes (30): ClusterConfig, Neros.Organization.Api, HttpMessageHandler, RouteConfig, SecurityKey, WebApplication, WebApplicationBuilder, GatewayHost (+22 more)

### Community 3 - "http"
Cohesion: 0.05
Nodes (36): Actividad y refresco, Botones y controles de icono, Campos y acceso, Catalogo comercial del acceso, Cierre ship y limites de verificacion, Colors, Components, Do: (+28 more)

### Community 4 - "_Imports.razor"
Cohesion: 0.18
Nodes (12): Attribute, Neros.Api.Seguridad, Neros.Application.Cuenta, Neros.Contracts.Cuenta, Neros.Application.Seguridad, Neros.Persistence.Seguridad, Neros.Api.Controllers, Neros.Application.Autenticacion (+4 more)

### Community 6 - "ReconnectModal.razor.js"
Cohesion: 0.32
Nodes (6): handleReconnectStateChanged(), reconnectModal, resumeButton, retry(), retryButton, retryWhenDocumentBecomesVisible()

### Community 7 - "App.razor"
Cohesion: 0.29
Nodes (6): HeadOutlet, ImportMap, ReconnectModal, ResourcePreloader, Routes, TextosCliente

### Community 8 - "AzureDevOps"
Cohesion: 0.33
Nodes (5): AzureDevOps, postman, ADO_MCP_AUTH_TOKEN, npx, @azure-devops/mcp

### Community 9 - "Routes.razor"
Cohesion: 0.18
Nodes (10): AuthorizeRouteView, FocusOnNavigate, Found, Acciones, ChildContent, EstadoVacio, Icono, TextosComunes (+2 more)

### Community 10 - "WeatherForecast.cs"
Cohesion: 0.50
Nodes (3): WeatherForecast, Neros.Api, DateOnly

### Community 11 - "Error.razor"
Cohesion: 0.33
Nodes (7): DelegatingHandler, CancellationToken, HttpRequestMessage, HttpResponseMessage, Task, CorrelationHandler, ResilienceFailureHandler

### Community 16 - "Counter.razor"
Cohesion: 0.13
Nodes (15): Acceso e inicio multiempresa, Alcance, Alta inicial, Arquitectura, Configuracion local, Contrato HTTP, Cuenta, permisos y auditoria, Ejecucion (+7 more)

### Community 17 - "Weather.razor"
Cohesion: 0.07
Nodes (36): Claro, GeneratedRegex, HttpStatusCode, IAsyncLifetime, IClassFixture, AccesoConcedido, SolicitudCambioClave, Oscuro (+28 more)

### Community 18 - "NavMenu"
Cohesion: 0.18
Nodes (10): MenuUsuario, NavMenu, Icono, IHttpContextAccessor, IStringLocalizer<TextosComunes>, LayoutComponentBase, Marca, NavigationManager (+2 more)

### Community 19 - "NavLink"
Cohesion: 0.29
Nodes (6): NavLink, Icono, IHttpContextAccessor, IStringLocalizer<TextosComunes>, Marca, TextosComunes

### Community 20 - "Home.razor"
Cohesion: 0.14
Nodes (13): ActividadReciente, ContextoEmpresa, ModulosEnPreparacion, OnInitializedAsync, Aviso, ClienteNeros, Icono, IHttpContextAccessor (+5 more)

### Community 21 - "C# Async â€” Neros"
Cohesion: 0.22
Nodes (8): Banned, By Layer, C# Async - Neros, Cancellation, Exceptions, Names, Parallelism, Return Types

### Community 22 - "Program.cs"
Cohesion: 0.09
Nodes (27): Acceso, HttpMethod, CancellationToken, Error, Guid, HttpRequestMessage, HttpResponseMessage, IReadOnlyList (+19 more)

### Community 23 - "ReconnectModal.razor"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 24 - "NotFound.razor"
Cohesion: 0.11
Nodes (10): Neros.Blazor.Components.Shared, Navegador, IEnumerable, IReadOnlyList, CatalogoModulos, ModuloErp, DateTime, Presentacion (+2 more)

### Community 25 - "CONVENTIONS.md"
Cohesion: 0.09
Nodes (19): DateTimeOffset, Guid, JsonElement, IntegrationEnvelope, CancellationToken, Guid, Task, IEventBus (+11 more)

### Community 26 - "package.json"
Cohesion: 0.15
Nodes (12): lucide-static, dependencies, lucide-static, devDependencies, tailwindcss, name, private, scripts (+4 more)

### Community 27 - "copilot-instructions.md"
Cohesion: 0.40
Nodes (4): D-02: Gateway y host Organization, Ejecucion local, Evidencia y limites, Implementacion

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
Cohesion: 0.28
Nodes (4): Módulo `compatibilidad`, Crear Script SQL - Neros, Reglas, Validacion

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
Cohesion: 0.15
Nodes (24): DbSet, ICollection, IdentityUser, IdentityUserContext, ModelBuilder, NerosDbContext, DateTime, Guid (+16 more)

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
Cohesion: 0.12
Nodes (18): IReadOnlyCollection, Fact, Task, OrganizationMigrationTests, Guid, IEnumerable, IReadOnlyList, EmpresaMigracion (+10 more)

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
Cohesion: 0.25
Nodes (7): Detalle, OnInitialized, ChildContent, IStringLocalizer<TextosSistema>, PageTitle, PaginaEstado, TextosSistema

### Community 79 - "Dev Agent"
Cohesion: 0.20
Nodes (9): Anti-Patterns, Dev Agent, Entrega esperada, Mision, Reglas de implementacion, Required Context, Skills To Load, Validacion (+1 more)

### Community 80 - "CODEX.md"
Cohesion: 0.22
Nodes (7): Arquitectura Objetivo, Comandos Canonicos, Entrega, Flujo Recomendado, Prioridad, Reglas Duras, Seleccion De Skill

### Community 81 - "Empresas.razor"
Cohesion: 0.13
Nodes (14): FilaEmpresa, OnInitializedAsync, Acciones, Aviso, ChildContent, ClienteNeros, EmpresaDisponible, EstadoVacio (+6 more)

### Community 82 - "ANTIGRAVITY.md"
Cohesion: 0.25
Nodes (6): Comandos Canonicos, Fuentes De Verdad, Modelo Del Proyecto, Reglas Duras, Roles Equivalentes, Workflow

### Community 83 - "Neros.Blazor.Components.Features.Auth"
Cohesion: 0.29
Nodes (6): Accion, ActividadAcceso, EstadoVacio, Icono, IStringLocalizer<TextosInicio>, TextosInicio

### Community 84 - "Login.razor"
Cohesion: 0.13
Nodes (14): CatalogoAcceso, AntiforgeryToken, Icono, IStringLocalizer<TextosAcceso>, IStringLocalizer<TextosComunes>, IStringLocalizer<TextosPrivacidad>, Marca, PageTitle (+6 more)

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
Cohesion: 0.40
Nodes (4): Icono, Marca, Preferencias, TextosComunes

### Community 96 - "AuthLayout.razor"
Cohesion: 0.50
Nodes (3): IStringLocalizer<TextosComunes>, LayoutComponentBase, TextosComunes

### Community 97 - "SelectorTema.razor"
Cohesion: 0.40
Nodes (4): AntiforgeryToken, Icono, IStringLocalizer<TextosEmpresas>, TextosEmpresas

### Community 100 - "ReconnectModal.razor"
Cohesion: 0.40
Nodes (4): IStringLocalizer<TextosSistema>, PageTitle, PaginaEstado, TextosSistema

### Community 101 - "LoginAmbient.razor"
Cohesion: 0.33
Nodes (5): Icono, IStringLocalizer<TextosAcceso>, IStringLocalizer<TextosComunes>, TextosAcceso, TextosComunes

### Community 102 - "LoginHeader.razor"
Cohesion: 0.50
Nodes (3): Icono, IStringLocalizer<TextosInicio>, TextosInicio

### Community 103 - "LoginModuleTile.razor"
Cohesion: 0.33
Nodes (5): Icono, IStringLocalizer<TextosComunes>, IStringLocalizer<TextosInicio>, TextosComunes, TextosInicio

### Community 104 - "Icono.razor"
Cohesion: 0.50
Nodes (3): Icono, IStringLocalizer<TextosComunes>, TextosComunes

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
Cohesion: 0.21
Nodes (12): ActionResult, CancellationToken, EnableRateLimiting, Guid, HttpGet, HttpPost, IActionResult, IReadOnlyList (+4 more)

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
Cohesion: 0.33
Nodes (6): Base de datos Neros ERP, Base existente creada con los scripts anteriores, Cambios de esquema, Instalación nueva, Reversión, Seguridad y operación

### Community 116 - "FoundationHttpTests.cs"
Cohesion: 0.14
Nodes (10): ConcurrentQueue, EventId, ILoggerProvider, LogLevel, Exception, Func, IDisposable, ILogger (+2 more)

### Community 117 - "ServiceAuthentication.cs"
Cohesion: 0.11
Nodes (14): AuthenticationHandler, AuthorizationPolicyBuilder, IAuthorizationRequirement, Microsoft.AspNetCore.Authorization, AuthenticationSchemeOptions, AutenticacionSesion, IServiceCollection, Uri (+6 more)

### Community 118 - ".SendAsync"
Cohesion: 0.17
Nodes (11): ASPNETCORE_ENVIRONMENT, Identity__Authority, Services__Compatibility, Services__Organization, applicationUrl, commandName, environmentVariables, launchBrowser (+3 more)

### Community 119 - "_Imports.razor"
Cohesion: 0.11
Nodes (17): Microsoft.AspNetCore.Components.Authorization, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.Web.Virtualization, Microsoft.JSInterop, Neros.Blazor, Neros.Blazor.Components.Features.Privacidad (+9 more)

### Community 120 - ".TryFromAuthenticatedPrincipal"
Cohesion: 0.10
Nodes (21): AuthorizationHandler, AuthorizationHandlerContext, IReadOnlySet, ServicePermission, ClaimsPrincipal, DateTimeOffset, Guid, SecurityContext (+13 more)

### Community 121 - "Neros.ServiceDefaults"
Cohesion: 0.29
Nodes (4): Neros.ServiceDefaults, Neros.Tests, System.Diagnostics, SpanEvidence

### Community 122 - "NEROS: estado de ejecucion"
Cohesion: 0.17
Nodes (12): Completed gates, Current blockers, Current initiative, Important decisions, Migrations, NEROS: estado de ejecucion, Next action, Pending gates (+4 more)

### Community 123 - "ServicioIdentidad"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, Identity__Authority, applicationUrl, commandName, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 124 - "SecurityContext"
Cohesion: 0.18
Nodes (15): CancellationToken, Guid, IReadOnlyList, Task, IRepositorioEmpresas, CancellationToken, Guid, IReadOnlyList (+7 more)

### Community 125 - "EvidenceExporter"
Cohesion: 0.40
Nodes (5): Activity, BaseExporter, Batch, ExportResult, EvidenceExporter

### Community 126 - "AutenticacionSesion"
Cohesion: 0.40
Nodes (3): AuthenticateResult, Task, SesionValidada

### Community 127 - "D-01: contratos, amenazas y transicion"
Cohesion: 0.33
Nodes (6): Contratos de borde, D-01: contratos, amenazas y transicion, Gate D-01, Mensajeria e idempotencia, Migracion y autorizacion, Threat model y evidencia

### Community 128 - ".EjecutarAsync"
Cohesion: 0.47
Nodes (3): IServiceProvider, Task, ComandosAdministracion

### Community 130 - "EmpresaDisponible"
Cohesion: 0.06
Nodes (37): Columna, ColumnaReal, Esquema, IDisposable, ModuloTemporal, Campo, AntiforgeryToken, Aviso (+29 more)

### Community 132 - "System.Security.Claims"
Cohesion: 0.03
Nodes (79): 10. Concurrencia, 11. Escala, 12. Fechas y zonas horarias, 13. Historial de datos maestros, 14. Auditoría inmutable, 15. Cuenta de usuario (acordado), 16. Sesiones, 17. MFA (+71 more)

### Community 133 - "CONVENTIONS.md"
Cohesion: 0.17
Nodes (11): API Y Application, Base De Datos, Capas, Dependencias Permitidas, Estructura De Neros.Blazor, Frontend, Idioma Y Nombres, Idiomas (i18n) (+3 more)

### Community 134 - "SelectorTema.razor"
Cohesion: 0.50
Nodes (3): Icono, IStringLocalizer<TextosComunes>, TextosComunes

### Community 138 - "DesplegadorBaseDatos"
Cohesion: 0.06
Nodes (44): Base, bool, Cadena, char, Neros.Database.Deploy, Exception, RegexOptions, SqlCommand (+36 more)

### Community 139 - ".CambiarAsync"
Cohesion: 0.10
Nodes (14): IStringLocalizer, HttpContext, IAntiforgery, IResult, IServiceCollection, Task, WebApplication, ConfiguracionLocalizacion (+6 more)

### Community 140 - ".InicioAsync"
Cohesion: 0.23
Nodes (10): Authorize, ActionResult, CancellationToken, HttpGet, HttpPost, IActionResult, Task, AdministracionUsuariosController (+2 more)

### Community 141 - "CancellationToken"
Cohesion: 0.13
Nodes (14): IStringLocalizer<TextosAdministracion>, CargarAsync, EnlacePagina, OnInitializedAsync, OperarAsync, AntiforgeryToken, Aviso, ClienteNeros (+6 more)

### Community 142 - "RepositorioEmpresas"
Cohesion: 0.35
Nodes (6): CancellationToken, Guid, IReadOnlyList, Task, IServicioAdministracionUsuarios, IServicioCuenta

### Community 143 - "PlanMigracion.cs"
Cohesion: 0.27
Nodes (10): ResultadoCambioClave, ContextoCliente, string, RegistroAuditoria, CancellationToken, Guid, IReadOnlyList, string (+2 more)

### Community 144 - "Neros.Contracts.Autenticacion"
Cohesion: 0.14
Nodes (13): Descripcion, OnInitializedAsync, AntiforgeryToken, Aviso, ClienteNeros, EstadoVacio, Icono, IHttpContextAccessor (+5 more)

### Community 145 - "Convenciones SQL de Neros"
Cohesion: 0.22
Nodes (9): Análisis estático (runner), Columnas estándar de tablas de negocio, Convenciones SQL de Neros, Encabezado obligatorio (runner), Estructura por modulo, Nombres de objetos, Tipos de datos, Transacciones (+1 more)

### Community 146 - "EmpresaDisponible"
Cohesion: 0.14
Nodes (10): Neros.Persistence.Privacidad, Neros.Contracts.Privacidad, Neros.Gateway, Neros.Application.Privacidad, Neros.Persistence, Program, HashContenido, VigenteProjection (+2 more)

### Community 147 - "ServicioIdentidad"
Cohesion: 0.27
Nodes (7): IdentityResult, EstadoAdministracion, CancellationToken, Estado, string, Task, ServicioAdministracionUsuarios

### Community 148 - ".Create"
Cohesion: 0.08
Nodes (33): ActionResult, AllowAnonymous, CancellationToken, HttpGet, HttpPost, IActionResult, IReadOnlyList, PermitirConClaveTemporal (+25 more)

### Community 149 - "System.Text.Json"
Cohesion: 0.21
Nodes (6): ClaimsPrincipal, Guid, HttpContext, IApplicationBuilder, string, SesionHttp

### Community 150 - "Guía de despliegue de base de datos"
Cohesion: 0.25
Nodes (8): Adoptar una base existente (baseline), Base nueva, Cambios destructivos, Comandos, Flujo de un script, Guía de despliegue de base de datos, Pruebas y validación continua, Registro de versiones

### Community 151 - "FoundationHttpTests"
Cohesion: 0.39
Nodes (5): Fact, InlineData, Task, Theory, FoundationHttpTests

### Community 152 - "FoundationMessagingTests"
Cohesion: 0.23
Nodes (8): App, IHost, IHostBuilder, IWebHostBuilder, AccesoController, FabricaApi, FabricaBlazor, WebApplicationFactory

### Community 153 - ".LeerEmpresasAsync"
Cohesion: 0.24
Nodes (9): CancellationToken, IReadOnlyList, string, Task, ServicioIdentidad, DateTimeOffset, SesionesUsuario, TimeSpan (+1 more)

### Community 154 - "SelectorIdioma.razor"
Cohesion: 0.33
Nodes (5): AntiforgeryToken, Icono, IStringLocalizer<TextosComunes>, NavigationManager, TextosComunes

### Community 155 - "ADR-0004: plan maestro, esquema por scripts SQL y salto autorizado de gates"
Cohesion: 0.40
Nodes (5): ADR-0004: plan maestro, esquema por scripts SQL y salto autorizado de gates, Consecuencias, Contexto, Decisiones, Verificacion

### Community 156 - "7. Runner, versionamiento y rollback"
Cohesion: 0.40
Nodes (5): 7.1. Runner, 7.2. Tabla de versión (una por base), 7.3. Transacciones, 7.4. Rollback, 7. Runner, versionamiento y rollback

### Community 157 - "ADR-0003: foundation de desarrollo y contratos de borde"
Cohesion: 0.50
Nodes (4): ADR-0003: foundation de desarrollo y contratos de borde, Alternativas y consecuencias, Decisiones, Verificacion y limites

### Community 162 - "Changelog `compatibilidad`"
Cohesion: 0.50
Nodes (3): Changelog `compatibilidad`, V0001 — 2026-09-25, V0002 — 2026-09-25

### Community 163 - "0. Control de cambios"
Cohesion: 0.67
Nodes (3): 0.1. Correcciones respecto a la versión 2.0, 0.2. Decisiones del usuario ya tomadas (2026-09-25), 0. Control de cambios

### Community 164 - "4. Scripts SQL como fuente de verdad"
Cohesion: 0.67
Nodes (3): 4.1. Estructura, 4.2. Reglas, 4. Scripts SQL como fuente de verdad

### Community 165 - "PARTE XI — DEFINITION OF DONE"
Cohesion: 0.67
Nodes (3): Legal (funcionalidades reguladas), PARTE XI — DEFINITION OF DONE, Técnica

### Community 167 - "FoundationHttp"
Cohesion: 0.22
Nodes (6): object, IApplicationBuilder, IServiceCollection, string, WebApplication, FoundationHttp

### Community 168 - ".TryHandleAsync"
Cohesion: 0.20
Nodes (8): HealthReport, IExceptionHandler, CancellationToken, Exception, HttpContext, Task, UnhandledExceptionHandler, ValueTask

### Community 169 - ".DosHostsHttp_PropaganCorrelacionYExportanMismaTraza"
Cohesion: 0.33
Nodes (5): Fact, Task, Uri, WebApplication, FoundationTelemetryTests

### Community 170 - "MenuUsuario.razor"
Cohesion: 0.33
Nodes (5): AntiforgeryToken, Icono, IHttpContextAccessor, IStringLocalizer<TextosComunes>, TextosComunes

### Community 171 - "Idiomas"
Cohesion: 0.16
Nodes (12): ActionResult, AllowAnonymous, CancellationToken, EnableRateLimiting, HttpGet, HttpPost, IActionResult, PermitirConClaveTemporal (+4 more)

### Community 172 - "ContratosCuenta.cs"
Cohesion: 0.29
Nodes (6): string, CodigosCuenta, ErrorOperacion, PaginaUsuarios, SesionActiva, UsuarioAdministrado

### Community 173 - "FoundationTelemetry"
Cohesion: 0.40
Nodes (3): IHostApplicationBuilder, IHttpClientBuilder, FoundationTelemetry

### Community 174 - ".RevocarAsync"
Cohesion: 0.50
Nodes (3): CancellationToken, Guid, Task

### Community 177 - ".InicioAsync"
Cohesion: 0.18
Nodes (11): WeatherForecastController, ControllerBase, ActionResult, CancellationToken, Guid, HttpGet, HttpPost, IReadOnlyList (+3 more)

### Community 178 - "System.Text.Json"
Cohesion: 0.15
Nodes (9): Neros.Organization.Migration, Neros.Messaging.Abstractions, IStringLocalizer<TextosComunes>, TextosComunes, System.Text.Json, Request, ColumnaOrigen, EmpresaOrigen (+1 more)

### Community 179 - "PaginaAceptacionLegal.razor"
Cohesion: 0.12
Nodes (15): MarkdownSeguro, OnInitializedAsync, AntiforgeryToken, Aviso, ClienteNeros, DocumentoLegalPendiente, DocumentoLegalPublicado, Icono (+7 more)

### Community 180 - "RepositorioEmpresas"
Cohesion: 0.38
Nodes (6): CancellationToken, Guid, IQueryable, IReadOnlyList, Task, RepositorioEmpresas

### Community 181 - "EndpointsSesion.cs"
Cohesion: 0.23
Nodes (7): Neros.Contracts.Seguridad, Neros.Blazor.Localizacion, Neros.Blazor.Servicios, Microsoft.Extensions.Localization, Neros.Blazor.Components, string, CodigosPermiso

### Community 182 - "PaginaCookies.razor"
Cohesion: 0.17
Nodes (11): OnInitializedAsync, Aviso, ClienteNeros, DefinicionCookiePublica, Icono, IStringLocalizer<TextosComunes>, IStringLocalizer<TextosPrivacidad>, PageTitle (+3 more)

### Community 183 - "Base multiempresa de Neros ERP"
Cohesion: 0.25
Nodes (8): Base multiempresa de Neros ERP, Criterio de aceptacion, Decisiones gerenciales necesarias, Lo que no incluye, Puesta en marcha, Resumen ejecutivo, Siguiente etapa, Valor entregado

### Community 184 - "Permisos"
Cohesion: 0.40
Nodes (5): IReadOnlyDictionary, IEnumerable, IReadOnlyList, string, Permisos

## Knowledge Gaps
- **993 isolated node(s):** `$schema`, `frontend-design@claude-plugins-official`, `feature-dev@claude-plugins-official`, `_comment`, `skillsPath` (+988 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **27 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Neros.Tests` connect `Neros.ServiceDefaults` to `EmpresaDisponible`, `_Imports.razor`, `Weather.razor`, `System.Text.Json`, `EmpresaDisponible`?**
  _High betweenness centrality (0.050) - this node is a cross-community bridge._
- **Why does `System.Security.Claims` connect `_Imports.razor` to `Neros.ServiceDefaults`, `EmpresaDisponible`, `EndpointsSesion.cs`, `_Imports.razor`?**
  _High betweenness centrality (0.023) - this node is a cross-community bridge._
- **Why does `Neros.ServiceDefaults` connect `Neros.ServiceDefaults` to `http`, `_Imports.razor`, `Error.razor`, `EmpresaDisponible`, `ServiceAuthentication.cs`, `EndpointsSesion.cs`?**
  _High betweenness centrality (0.021) - this node is a cross-community bridge._
- **What connects `$schema`, `frontend-design@claude-plugins-official`, `feature-dev@claude-plugins-official` to the rest of the system?**
  _993 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Neros.Shared` be split into smaller, more focused modules?**
  _Cohesion score 0.05333333333333334 - nodes in this community are weakly interconnected._
- **Should `settings.json` be split into smaller, more focused modules?**
  _Cohesion score 0.11764705882352941 - nodes in this community are weakly interconnected._
- **Should `http` be split into smaller, more focused modules?**
  _Cohesion score 0.06666666666666667 - nodes in this community are weakly interconnected._