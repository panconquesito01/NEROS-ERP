# Graph Report - NEROS ERP  (2026-09-25)

## Corpus Check
- 198 files · ~73,763 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1616 nodes · 1998 edges · 162 communities (143 shown, 19 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 39 edges (avg confidence: 0.76)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `57efeadf`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Neros.Tests.csproj
- AccesoMultiempresaTests
- FoundationHttp
- Components
- .Crear
- ClienteNeros
- .TryFromAuthenticatedPrincipal
- System.Security.Claims
- Neros ERP - Estrategia Gerencial, Comercial y de Producto
- FoundationHttpTests.cs
- NEROS ERP: diagnostico de arquitectura
- Neros.ServiceDefaults
- NEROS ERP: arquitectura objetivo distribuida
- .IniciarAsync
- settings.json
- NEROS ERP: roadmap de plataforma distribuida
- Stale Issues And PRs Triage
- http
- http
- System.Text.Json
- PaginaEmpresas.razor
- AGENTS.md
- Diagnose - Neros
- .NET Best Practices - Neros
- PaginaInicio.razor
- Presentacion
- NerosDbContext
- EmpresaDisponible
- .Validar
- _Imports.razor
- Acceso e inicio multiempresa
- CancellationToken
- Architecture Blueprint Generator - Neros
- Refactor - Neros
- IntegrationEnvelope
- PaginaAcceso.razor
- .CambiarAsync
- package.json
- CONVENTIONS.md
- README.md
- Acquire Codebase Knowledge - Neros
- NEROS_EXECUTION_STATUS.md
- ASP.NET MVC/Razor Legacy Reference - Neros
- EF Core / SQL Server - Neros
- environmentVariables
- AutenticacionSesion
- Routes.razor
- EndpointsSesion.cs
- MainLayout.razor
- Reempaque Y Saldos Pendientes - Neros
- C# Async - Neros
- Security Review - Neros
- Backend Agent
- Dev Agent
- Frontend Agent
- QA Agent
- .InicioAsync
- http
- Backend Agent
- Dev Agent
- Frontend Agent
- QA Agent
- ◈ Ecosistema Neros
- ServiceAuthentication.cs
- CODEX.md
- Planner Agent
- NEROS: estado de ejecucion
- Validacion de acceso multiempresa
- Authoring Agent Skills - Neros
- Authoring Agents - Neros
- C# Async - Neros
- Authoring Instructions Files - Neros
- RedAcceso
- ServicioIdentidad
- Create Implementation Plan - Neros
- Planner Agent
- README.md
- 🎯 Lo que diferencia a Neros
- PaginaError.razor
- .LeerEmpresasAsync
- ANTIGRAVITY.md
- Base multiempresa de Neros ERP
- Configuracion por entorno y secretos de Neros
- .ObtenerInicioAsync
- ReconnectModal.razor.js
- Reviewing Code - Neros
- .Create
- FoundationMessagingTests
- 🗺️ Roadmap
- Microsoft.AspNetCore.Authorization
- App.razor
- NavMenu.razor
- ADR-0002: Distributed Modular ERP Platform
- GitHub Actions - Neros
- SignalR - Neros
- ActividadReciente.razor
- experience.js
- EvidenceExporter
- AzureDevOps
- copilot-instructions.md
- .EjecutarAsync
- CLAUDE.md
- ADR-0001: evolucion incremental a monolito modular
- D-01: contratos, amenazas y transicion
- C# - Neros
- EF Core - Neros
- Reports And PDF - Neros
- Crear Script SQL - Neros
- CatalogoAcceso.razor
- ModulosEnPreparacion.razor
- SelectorIdioma.razor
- Idiomas
- PlanMigracion.cs
- Antigravity Bridge - Neros
- Codex Bridge - Neros
- GEMINI.md
- Blazor - Neros
- Security - Neros
- Crear Endpoint API - Neros
- Crear Feature Blazor - Neros
- Crear Caso De Uso - Neros
- FilaEmpresa.razor
- PaginaNoEncontrada.razor
- PaginaEstado.razor
- 🧠 Diseñado para toda la organización
- WeatherForecast.cs
- Antigravity Agents - Neros
- Codex Agents - Neros
- AGENTS.md
- Base de datos Neros ERP
- Architect Mode - Neros
- Code Review Mode - Neros
- Debug Mode - Neros
- Refactor Mode - Neros
- ContextoEmpresa.razor
- AuthLayout.razor
- Aviso.razor
- SelectorTema.razor
- AGENTS.md
- ⚡ Neros convierte procesos aislados en una sola operación conectada
- 🌐 La visión a largo plazo
- TextosAcceso.cs
- TextosEmpresas.cs
- TextosInicio.cs
- Class1.cs
- Class1.cs
- Class1.cs
- Class1.cs
- TextosSistema.cs
- Planner Fast - Neros
- Marca.razor
- Preferencias.razor
- theme-init.js
- 🏗️ Arquitectura
- 🚀 Desarrollo local
- javascript-frontend.instructions.md
- markdown.instructions.md
- ReconnectModal.razor
- EstadoVacio.razor

## God Nodes (most connected - your core abstractions)
1. `NEROS ERP: arquitectura objetivo distribuida` - 18 edges
2. `AccesoMultiempresaTests` - 17 edges
3. `NEROS ERP: diagnostico de arquitectura` - 16 edges
4. `Neros ERP - Estrategia Gerencial, Comercial y de Producto` - 15 edges
5. `.NET Best Practices - Neros` - 14 edges
6. `System.Security.Claims` - 13 edges
7. `ClienteNeros` - 13 edges
8. `EmpresaDisponible` - 13 edges
9. `Neros.ServiceDefaults` - 13 edges
10. `NEROS ERP: roadmap de plataforma distribuida` - 13 edges

## Surprising Connections (you probably didn't know these)
- `FabricaApi` --references--> `AccesoController`  [EXTRACTED]
  Neros/tests/Neros.Tests/EntornoPruebas.cs → Neros/Neros.Api/Controllers/AccesoController.cs
- `ServicioIdentidad` --implements--> `IServicioIdentidad`  [EXTRACTED]
  Neros/Neros.Persistence/Seguridad/ServicioIdentidad.cs → Neros/Neros.Application/Autenticacion/IServicioIdentidad.cs
- `RepositorioEmpresas` --implements--> `IRepositorioEmpresas`  [EXTRACTED]
  Neros/Neros.Persistence/Seguridad/RepositorioEmpresas.cs → Neros/Neros.Application/Autenticacion/IServicioIdentidad.cs
- `ServicioIdentidad` --references--> `Usuario`  [EXTRACTED]
  Neros/Neros.Persistence/Seguridad/ServicioIdentidad.cs → Neros/Neros.Persistence/Seguridad/ModeloSeguridad.cs
- `NerosDbContext` --references--> `Empresa`  [EXTRACTED]
  Neros/Neros.Persistence/NerosDbContext.cs → Neros/Neros.Persistence/Seguridad/ModeloSeguridad.cs

## Import Cycles
- None detected.

## Communities (162 total, 19 thin omitted)

### Community 0 - "Neros.Tests.csproj"
Cohesion: 0.05
Nodes (46): net10.0, Microsoft.NET.Sdk.Web, Neros.Application, net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk.Web, Neros.Contracts (+38 more)

### Community 1 - "AccesoMultiempresaTests"
Cohesion: 0.10
Nodes (25): App, Claro, IAsyncLifetime, IClassFixture, IHost, IHostBuilder, IPage, IWebHostBuilder (+17 more)

### Community 2 - "FoundationHttp"
Cohesion: 0.06
Nodes (29): DelegatingHandler, HealthReport, IApplicationBuilder, IExceptionHandler, IHostApplicationBuilder, IHttpClientBuilder, CancellationToken, Exception (+21 more)

### Community 3 - "Components"
Cohesion: 0.05
Nodes (36): Actividad y refresco, Botones y controles de icono, Campos y acceso, Catalogo comercial del acceso, Cierre ship y limites de verificacion, Colors, Components, Do: (+28 more)

### Community 4 - ".Crear"
Cohesion: 0.11
Nodes (21): ClusterConfig, WebApplication, WebApplicationBuilder, GatewayHost, string, WebApplication, WebApplicationBuilder, OrganizationHost (+13 more)

### Community 5 - "ClienteNeros"
Cohesion: 0.15
Nodes (16): HttpMethod, CancellationToken, Guid, HttpRequestMessage, IReadOnlyList, Task, ClienteNeros, CancellationToken (+8 more)

### Community 6 - ".TryFromAuthenticatedPrincipal"
Cohesion: 0.10
Nodes (20): AuthorizationHandler, AuthorizationHandlerContext, Claim, IReadOnlySet, ClaimsPrincipal, DateTimeOffset, Guid, SecurityContext (+12 more)

### Community 7 - "System.Security.Claims"
Cohesion: 0.12
Nodes (16): ControllerBase, Neros.Api.Seguridad, Neros.Persistence.Seguridad, Neros.Api.Controllers, Neros.Application.Autenticacion, Neros.Contracts.Autenticacion, Neros.Persistence, EmpresasController (+8 more)

### Community 8 - "Neros ERP - Estrategia Gerencial, Comercial y de Producto"
Cohesion: 0.08
Nodes (25): 10. Comercializacion y aliados, 11. Futura web comercial independiente, 12. Materiales y gobierno comercial, 13. Condiciones para avanzar, 1. Decision ejecutiva, 2. Que es Neros, 3. Estado real del producto, 4. Catalogo comercial propuesto (+17 more)

### Community 9 - "FoundationHttpTests.cs"
Cohesion: 0.11
Nodes (15): ConcurrentQueue, EventId, Func, IDisposable, ILoggerProvider, LogLevel, Exception, Fact (+7 more)

### Community 10 - "NEROS ERP: diagnostico de arquitectura"
Cohesion: 0.09
Nodes (23): 10. Refactorizaciones necesarias, 11. Roadmap tecnico, 12. Roadmap funcional, 13. Dependencias funcionales, 14. Riesgos y bloqueos, 1. Estado actual, 2. Modulos existentes, 3. Arquitectura encontrada (+15 more)

### Community 11 - "Neros.ServiceDefaults"
Cohesion: 0.13
Nodes (12): Neros.ServiceDefaults, Neros.Tests, Neros.Organization.Api, HttpMessageHandler, int, ContextoAutorizado, Task, Program (+4 more)

### Community 12 - "NEROS ERP: arquitectura objetivo distribuida"
Cohesion: 0.11
Nodes (18): 10. NEROS Jobs y archivos, 11. Configuracion, contenedores, discovery y CI/CD, 12. Observabilidad distribuida, 13. Data Platform, Search y localizaciones, 14. Escalabilidad y prueba de autonomia, 15. Estructura .NET propuesta y reutilizacion, 16. Transicion sin ruptura, 17. Gobierno y entregables (+10 more)

### Community 13 - ".IniciarAsync"
Cohesion: 0.15
Nodes (13): AllowAnonymous, EnableRateLimiting, IActionResult, ActionResult, CancellationToken, HttpGet, HttpPost, Task (+5 more)

### Community 14 - "settings.json"
Cohesion: 0.12
Nodes (16): agentsPath, buildCommand, _comment, _comment_hooks, defaultLanguage, enabledPlugins, feature-dev@claude-plugins-official, frontend-design@claude-plugins-official (+8 more)

### Community 15 - "NEROS ERP: roadmap de plataforma distribuida"
Cohesion: 0.12
Nodes (17): 10. Decisiones y lista de aplazamiento, 11. Trazabilidad del roadmap anterior, 12. I-01: cierre historico conservado, 1. Orden y fundamento en el repositorio, 2. Top 10 y reglas de prioridad, 3. Foundation y Platform Infrastructure: ejecucion exacta, 4. Identity / Organization: ownership y corte, 5. Master Data y primer vertical real (+9 more)

### Community 16 - "Stale Issues And PRs Triage"
Cohesion: 0.12
Nodes (13): Constraints, Daily Repo Status Report, Inputs To Fetch, Output, Agentic Workflows - Neros, File Format, Files, Rules (+5 more)

### Community 17 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 18 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 19 - "System.Text.Json"
Cohesion: 0.16
Nodes (9): Neros.Organization.Migration, Neros.Messaging.Abstractions, IStringLocalizer<TextosComunes>, TextosComunes, Request, ColumnaOrigen, EmpresaOrigen, InformeOrigen (+1 more)

### Community 20 - "PaginaEmpresas.razor"
Cohesion: 0.13
Nodes (14): FilaEmpresa, OnInitializedAsync, Acciones, Aviso, ChildContent, ClienteNeros, EmpresaDisponible, EstadoVacio (+6 more)

### Community 21 - "AGENTS.md"
Cohesion: 0.13
Nodes (13): Comandos canonicos, Direccion de dependencias, Estructura, Graphify, Proyecto, Puentes De Agente, Red flags que deben detener una implementacion, Reglas duras (+5 more)

### Community 22 - "Diagnose - Neros"
Cohesion: 0.13
Nodes (14): API Endpoint Fails, Blazor Page Fails, Common Failure Surfaces, Diagnose - Neros, Diagnostic Playbooks, Error Pattern Table, If Local Commands Are Allowed, If Local Commands Are Not Allowed (+6 more)

### Community 23 - ".NET Best Practices - Neros"
Cohesion: 0.13
Nodes (14): API Response Pattern, Architecture, Common Neros Code Smells And Fixes, Configuration, Data, Dependency Injection, DI Checklist, Errors (+6 more)

### Community 24 - "PaginaInicio.razor"
Cohesion: 0.14
Nodes (13): ActividadReciente, ContextoEmpresa, ModulosEnPreparacion, OnInitializedAsync, Aviso, ClienteNeros, Icono, IHttpContextAccessor (+5 more)

### Community 25 - "Presentacion"
Cohesion: 0.15
Nodes (7): Neros.Blazor.Components.Shared, IEnumerable, IReadOnlyList, CatalogoModulos, ModuloErp, Presentacion, TipoAviso

### Community 26 - "NerosDbContext"
Cohesion: 0.25
Nodes (12): DbSet, IdentityUser, IdentityUserContext, ModelBuilder, NerosDbContext, DateTimeOffset, Guid, Empresa (+4 more)

### Community 27 - "EmpresaDisponible"
Cohesion: 0.36
Nodes (7): IQueryable, EmpresaDisponible, CancellationToken, Guid, IReadOnlyList, Task, RepositorioEmpresas

### Community 28 - ".Validar"
Cohesion: 0.24
Nodes (8): IReadOnlyCollection, Fact, Task, OrganizationMigrationTests, Guid, IEnumerable, IReadOnlyList, ValidadorMigracion

### Community 29 - "_Imports.razor"
Cohesion: 0.14
Nodes (13): Microsoft.AspNetCore.Components.Authorization, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.Web.Virtualization, Microsoft.JSInterop, Neros.Blazor, Neros.Blazor.Components.Layout (+5 more)

### Community 30 - "Acceso e inicio multiempresa"
Cohesion: 0.14
Nodes (14): Acceso e inicio multiempresa, Alcance, Alta inicial, Arquitectura, Configuracion local, Contrato HTTP, Ejecucion, Interfaz (+6 more)

### Community 31 - "CancellationToken"
Cohesion: 0.31
Nodes (6): CancellationToken, Guid, IReadOnlyList, Task, IRepositorioEmpresas, IServicioIdentidad

### Community 32 - "Architecture Blueprint Generator - Neros"
Cohesion: 0.14
Nodes (13): Architecture Blueprint Generator - Neros, Blueprint Sections, Common Blueprint Mistakes, Data View, Deployment/Operations View, Inputs, Legacy Boundary, Output Quality (+5 more)

### Community 33 - "Refactor - Neros"
Cohesion: 0.14
Nodes (13): Checks, Common Refactor Smells, Do Not, Extract Application Use Case, Output, Principle, Refactor - Neros, Refactor Recipes (+5 more)

### Community 34 - "IntegrationEnvelope"
Cohesion: 0.20
Nodes (10): DateTimeOffset, Guid, JsonElement, IntegrationEnvelope, CancellationToken, Guid, Task, IEventBus (+2 more)

### Community 35 - "PaginaAcceso.razor"
Cohesion: 0.15
Nodes (12): CatalogoAcceso, AntiforgeryToken, Icono, IStringLocalizer<TextosAcceso>, IStringLocalizer<TextosComunes>, Marca, PageTitle, Preferencias (+4 more)

### Community 36 - ".CambiarAsync"
Cohesion: 0.15
Nodes (9): IStringLocalizer, HttpContext, IAntiforgery, IResult, IServiceCollection, Task, WebApplication, ConfiguracionLocalizacion (+1 more)

### Community 37 - "package.json"
Cohesion: 0.15
Nodes (12): lucide-static, dependencies, lucide-static, devDependencies, tailwindcss, name, private, scripts (+4 more)

### Community 38 - "CONVENTIONS.md"
Cohesion: 0.15
Nodes (11): API Y Application, Base De Datos, Capas, Dependencias Permitidas, Estructura De Neros.Blazor, Frontend, Idioma Y Nombres, Idiomas (i18n) (+3 more)

### Community 40 - "Acquire Codebase Knowledge - Neros"
Cohesion: 0.15
Nodes (12): Acquire Codebase Knowledge - Neros, Architecture Map, Common Discovery Mistakes, Escalate To, Evidence To Prefer, Exploration Workflow, Goal, Layer Ownership Guide (+4 more)

### Community 41 - "NEROS_EXECUTION_STATUS.md"
Cohesion: 0.18
Nodes (8): ADR-0003: foundation de desarrollo y contratos de borde, Alternativas y consecuencias, Decisiones, Verificacion y limites, D-02: Gateway y host Organization, Ejecucion local, Evidencia y limites, Implementacion

### Community 42 - "ASP.NET MVC/Razor Legacy Reference - Neros"
Cohesion: 0.17
Nodes (11): ASP.NET MVC/Razor Legacy Reference - Neros, Common Migration Traps, Do Not, Escalate To, Legacy Extraction Workflow, Migration Pattern, Output, Target For New Work (+3 more)

### Community 43 - "EF Core / SQL Server - Neros"
Cohesion: 0.17
Nodes (11): Common Mistakes And Fixes, EF Core / SQL Server - Neros, Manual SQL Script Template, Performance Checklist, Placement, Query Examples, Query Rules, Schema Changes (+3 more)

### Community 44 - "environmentVariables"
Cohesion: 0.17
Nodes (11): ASPNETCORE_ENVIRONMENT, Identity__Authority, Services__Compatibility, Services__Organization, applicationUrl, commandName, environmentVariables, launchBrowser (+3 more)

### Community 45 - "AutenticacionSesion"
Cohesion: 0.18
Nodes (9): AuthenticationHandler, AuthenticateResult, AuthenticationSchemeOptions, Task, AutenticacionSesion, AuthenticateResult, AuthenticationSchemeOptions, Task (+1 more)

### Community 46 - "Routes.razor"
Cohesion: 0.18
Nodes (10): AuthorizeRouteView, FocusOnNavigate, Found, Acciones, ChildContent, EstadoVacio, Icono, TextosComunes (+2 more)

### Community 47 - "EndpointsSesion.cs"
Cohesion: 0.24
Nodes (5): Neros.Blazor.Localizacion, Neros.Blazor.Servicios, Microsoft.Extensions.Localization, Neros.Blazor.Components, RutaLocal

### Community 48 - "MainLayout.razor"
Cohesion: 0.18
Nodes (10): NavMenu, AntiforgeryToken, Icono, IHttpContextAccessor, IStringLocalizer<TextosComunes>, LayoutComponentBase, Marca, NavigationManager (+2 more)

### Community 49 - "Reempaque Y Saldos Pendientes - Neros"
Cohesion: 0.18
Nodes (10): Arquitectura obligatoria, Diseno por capas, Entrega, No hacer, Objetivo funcional, Preflight, Reempaque Y Saldos Pendientes - Neros, Reglas de negocio (+2 more)

### Community 50 - "C# Async - Neros"
Cohesion: 0.18
Nodes (10): By Layer, C# Async - Neros, Common Mistakes And Fixes, ConfigureAwait, Defaults, Escalate To, Examples, Parallelism (+2 more)

### Community 51 - "Security Review - Neros"
Cohesion: 0.18
Nodes (10): Checklist, Escalate To, Identity And Authorization Checks, Neros Security Model, Neros Threat Patterns, Output, Review Workflow, Scope (+2 more)

### Community 52 - "Backend Agent"
Cohesion: 0.20
Nodes (9): Backend Agent, Checklist de diseno, Common Backend Findings To Prevent, Output Contract, Proposito, Reglas, Skills To Load, Validacion (+1 more)

### Community 53 - "Dev Agent"
Cohesion: 0.20
Nodes (9): Anti-Patterns, Dev Agent, Entrega esperada, Mision, Reglas de implementacion, Required Context, Skills To Load, Validacion (+1 more)

### Community 54 - "Frontend Agent"
Cohesion: 0.20
Nodes (9): Checklist UI, Frontend Agent, Output Contract, Proposito, Reglas, Skills To Load, UI Anti-Patterns, Validacion (+1 more)

### Community 55 - "QA Agent"
Cohesion: 0.20
Nodes (9): Checklist, Comandos, Mission, No Findings Output, QA Agent, Review Procedure, Salida, Severity Rubric (+1 more)

### Community 56 - ".InicioAsync"
Cohesion: 0.31
Nodes (7): ActionResult, CancellationToken, Guid, HttpGet, HttpPost, IReadOnlyList, Task

### Community 57 - "http"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, Identity__Authority, applicationUrl, commandName, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 58 - "Backend Agent"
Cohesion: 0.20
Nodes (9): Backend Agent, Checklist de diseno, Common Backend Findings To Prevent, Output Contract, Proposito, Reglas, Skills To Load, Validacion (+1 more)

### Community 59 - "Dev Agent"
Cohesion: 0.20
Nodes (9): Anti-Patterns, Dev Agent, Entrega esperada, Mision, Reglas de implementacion, Required Context, Skills To Load, Validacion (+1 more)

### Community 60 - "Frontend Agent"
Cohesion: 0.20
Nodes (9): Checklist UI, Frontend Agent, Output Contract, Proposito, Reglas, Skills To Load, UI Anti-Patterns, Validacion (+1 more)

### Community 61 - "QA Agent"
Cohesion: 0.20
Nodes (9): Checklist, Comandos, Mission, No Findings Output, QA Agent, Review Procedure, Salida, Severity Rubric (+1 more)

### Community 62 - "◈ Ecosistema Neros"
Cohesion: 0.20
Nodes (10): ◈ Ecosistema Neros, 📊 Neros Analytics, 🔌 Neros Connect, 🏢 Neros Core, 💰 Neros Finance, 🏭 Neros Manufacturing, 👥 Neros People, 🧾 Neros Sales (+2 more)

### Community 63 - "ServiceAuthentication.cs"
Cohesion: 0.22
Nodes (6): AuthorizationPolicyBuilder, IAuthorizationRequirement, IServiceCollection, Uri, ServiceAuthentication, ServicePermission

### Community 64 - "CODEX.md"
Cohesion: 0.22
Nodes (7): Arquitectura Objetivo, Comandos Canonicos, Entrega, Flujo Recomendado, Prioridad, Reglas Duras, Seleccion De Skill

### Community 65 - "Planner Agent"
Cohesion: 0.22
Nodes (8): Handoff Rules, Mission, Output Contract, Planner Agent, Required Context, Skills To Load, Stop Conditions, Workflow

### Community 66 - "NEROS: estado de ejecucion"
Cohesion: 0.22
Nodes (9): Completed gates, Current blockers, Current initiative, Important decisions, Migrations, NEROS: estado de ejecucion, Next action, Pending gates (+1 more)

### Community 67 - "Validacion de acceso multiempresa"
Cohesion: 0.22
Nodes (9): Casos cubiertos, Confirmacion de autenticacion y arranque, Errores de acceso sin recarga, Observaciones, Red animada y catalogo comercial, Resultado, Revision de acabado, Revision del login y estrategia comercial (+1 more)

### Community 68 - "Authoring Agent Skills - Neros"
Cohesion: 0.22
Nodes (8): Anti-patterns, Authoring Agent Skills - Neros, Body Structure, Commands To Reference, Folder Layout, Neros Defaults, Required Front Matter, Validation Checklist

### Community 69 - "Authoring Agents - Neros"
Cohesion: 0.22
Nodes (8): Authoring Agents - Neros, File Locations, Front Matter, Neros Defaults, Required Sections, Skills, Tooling Rules, Validation Checklist

### Community 70 - "C# Async - Neros"
Cohesion: 0.22
Nodes (8): Banned, By Layer, C# Async - Neros, Cancellation, Exceptions, Names, Parallelism, Return Types

### Community 71 - "Authoring Instructions Files - Neros"
Cohesion: 0.22
Nodes (8): Authoring Instructions Files - Neros, Body Structure, File Naming, Front Matter, Good applyTo Examples, Neros Content Rules, Validation Checklist, When To Create A New File

### Community 73 - "ServicioIdentidad"
Cohesion: 0.53
Nodes (4): CancellationToken, string, Task, ServicioIdentidad

### Community 74 - "Create Implementation Plan - Neros"
Cohesion: 0.22
Nodes (8): Acceptance Criteria, Common Plan Gaps To Avoid, Create Implementation Plan - Neros, Output, Plan Template, Planning Heuristics, Preflight, Rules

### Community 75 - "Planner Agent"
Cohesion: 0.22
Nodes (8): Handoff Rules, Mission, Output Contract, Planner Agent, Required Context, Skills To Load, Stop Conditions, Workflow

### Community 76 - "README.md"
Cohesion: 0.22
Nodes (8): 🤖 Desarrollo asistido por IA, El ERP que conecta operación, finanzas y crecimiento., 🚦 Estado del proyecto, ⚙️ Principios de ingeniería, Reglas fundamentales, 🧱 Stack tecnológico, 🛡️ Trazabilidad por diseño, ✦ Visión del producto

### Community 77 - "🎯 Lo que diferencia a Neros"
Cohesion: 0.22
Nodes (9): 01, 02, 03, 04, Evolutivo, Integrable, 🎯 Lo que diferencia a Neros, Modular (+1 more)

### Community 78 - "PaginaError.razor"
Cohesion: 0.25
Nodes (7): Detalle, OnInitialized, ChildContent, IStringLocalizer<TextosSistema>, PageTitle, PaginaEstado, TextosSistema

### Community 79 - ".LeerEmpresasAsync"
Cohesion: 0.43
Nodes (5): List, CancellationToken, Guid, Task, Program

### Community 80 - "ANTIGRAVITY.md"
Cohesion: 0.25
Nodes (6): Comandos Canonicos, Fuentes De Verdad, Modelo Del Proyecto, Reglas Duras, Roles Equivalentes, Workflow

### Community 81 - "Base multiempresa de Neros ERP"
Cohesion: 0.25
Nodes (8): Base multiempresa de Neros ERP, Criterio de aceptacion, Decisiones gerenciales necesarias, Lo que no incluye, Puesta en marcha, Resumen ejecutivo, Siguiente etapa, Valor entregado

### Community 82 - "Configuracion por entorno y secretos de Neros"
Cohesion: 0.25
Nodes (8): Como obtiene la conexion, Configuracion por entorno y secretos de Neros, Desarrollo y produccion, Efecto de Gitignore, Fuentes, Incorporar otro desarrollador, Resultado de la revision, Si aparece un secreto en Git

### Community 83 - ".ObtenerInicioAsync"
Cohesion: 0.43
Nodes (5): CancellationToken, Guid, IReadOnlyList, Task, ServicioEmpresas

### Community 84 - "ReconnectModal.razor.js"
Cohesion: 0.32
Nodes (6): handleReconnectStateChanged(), reconnectModal, resumeButton, retry(), retryButton, retryWhenDocumentBecomesVisible()

### Community 85 - "Reviewing Code - Neros"
Cohesion: 0.25
Nodes (7): Avoid, Common Review Findings In Neros, Output Format, Review Checklist, Review Procedure, Reviewing Code - Neros, Severity Rubric

### Community 86 - ".Create"
Cohesion: 0.29
Nodes (5): JsonElement, RequestFingerprint, InlineData, Theory, Utf8JsonWriter

### Community 87 - "FoundationMessagingTests"
Cohesion: 0.39
Nodes (4): Fact, Guid, JsonElement, FoundationMessagingTests

### Community 88 - "🗺️ Roadmap"
Cohesion: 0.25
Nodes (8): Etapa 01 — Foundation, Etapa 02 — Security, Etapa 03 — Persistence, Etapa 04 — Business Domains, Etapa 05 — Automation, Etapa 06 — Integrations, Etapa 07 — Analytics, 🗺️ Roadmap

### Community 89 - "Microsoft.AspNetCore.Authorization"
Cohesion: 0.29
Nodes (4): Neros.Gateway, Microsoft.AspNetCore.Authorization, Task, Program

### Community 90 - "App.razor"
Cohesion: 0.29
Nodes (6): HeadOutlet, ImportMap, ReconnectModal, ResourcePreloader, Routes, TextosCliente

### Community 91 - "NavMenu.razor"
Cohesion: 0.29
Nodes (6): NavLink, Icono, IHttpContextAccessor, IStringLocalizer<TextosComunes>, Marca, TextosComunes

### Community 92 - "ADR-0002: Distributed Modular ERP Platform"
Cohesion: 0.29
Nodes (7): ADR-0002: Distributed Modular ERP Platform, Alternativas, Consecuencias y costos aceptados, Contexto y evidencia, Decision, Decisiones acotadas pendientes, Transicion y criterios de aceptacion

### Community 93 - "GitHub Actions - Neros"
Cohesion: 0.29
Nodes (6): Agentic Workflows, Caching, Do Not, GitHub Actions - Neros, Neros Build Commands, Standard YAML Workflows

### Community 94 - "SignalR - Neros"
Cohesion: 0.29
Nodes (6): Client, Groups, Payloads, Server, SignalR - Neros, Validation

### Community 95 - "ActividadReciente.razor"
Cohesion: 0.29
Nodes (6): Accion, ActividadAcceso, EstadoVacio, Icono, IStringLocalizer<TextosInicio>, TextosInicio

### Community 97 - "EvidenceExporter"
Cohesion: 0.40
Nodes (5): Activity, BaseExporter, Batch, ExportResult, EvidenceExporter

### Community 98 - "AzureDevOps"
Cohesion: 0.33
Nodes (5): ADO_MCP_AUTH_TOKEN, npx, @azure-devops/mcp, AzureDevOps, postman

### Community 99 - "copilot-instructions.md"
Cohesion: 0.33
Nodes (5): Comandos canonicos, Fuente de verdad, Identidad del proyecto, Reglas duras, Workflow de agentes

### Community 100 - ".EjecutarAsync"
Cohesion: 0.47
Nodes (3): IServiceProvider, Task, ComandosAdministracion

### Community 101 - "CLAUDE.md"
Cohesion: 0.33
Nodes (5): Arquitectura objetivo, Comandos canonicos, Contexto, Graphify, Reglas de trabajo

### Community 102 - "ADR-0001: evolucion incremental a monolito modular"
Cohesion: 0.33
Nodes (6): ADR-0001: evolucion incremental a monolito modular, Alternativas, Compatibilidad y verificacion, Consecuencias, Contexto, Decision

### Community 103 - "D-01: contratos, amenazas y transicion"
Cohesion: 0.33
Nodes (6): Contratos de borde, D-01: contratos, amenazas y transicion, Gate D-01, Mensajeria e idempotencia, Migracion y autorizacion, Threat model y evidencia

### Community 104 - "C# - Neros"
Cohesion: 0.33
Nodes (5): After Editing, C# - Neros, Dependency Injection, Layers, Security And Data

### Community 105 - "EF Core - Neros"
Cohesion: 0.33
Nodes (5): EF Core - Neros, Placement, Queries, Schema, Transactions

### Community 106 - "Reports And PDF - Neros"
Cohesion: 0.33
Nodes (5): Data, Output, Placement, Reports And PDF - Neros, Rules

### Community 107 - "Crear Script SQL - Neros"
Cohesion: 0.33
Nodes (5): Contenido esperado, Crear Script SQL - Neros, Nombre sugerido, Reglas, Validacion

### Community 108 - "CatalogoAcceso.razor"
Cohesion: 0.33
Nodes (5): Icono, IStringLocalizer<TextosAcceso>, IStringLocalizer<TextosComunes>, TextosAcceso, TextosComunes

### Community 109 - "ModulosEnPreparacion.razor"
Cohesion: 0.33
Nodes (5): Icono, IStringLocalizer<TextosComunes>, IStringLocalizer<TextosInicio>, TextosComunes, TextosInicio

### Community 110 - "SelectorIdioma.razor"
Cohesion: 0.33
Nodes (5): AntiforgeryToken, Icono, IStringLocalizer<TextosComunes>, NavigationManager, TextosComunes

### Community 111 - "Idiomas"
Cohesion: 0.40
Nodes (4): IReadOnlyList, string, Idioma, Idiomas

### Community 112 - "PlanMigracion.cs"
Cohesion: 0.33
Nodes (5): EmpresaMigracion, EstadoCorrespondencia, GrupoMigracion, PlanMigracion, TenantMigracion

### Community 113 - "Antigravity Bridge - Neros"
Cohesion: 0.40
Nodes (4): Agents, Antigravity Bridge - Neros, Reglas, Skills

### Community 114 - "Codex Bridge - Neros"
Cohesion: 0.40
Nodes (4): Agents, Codex Bridge - Neros, Reglas, Skills

### Community 115 - "GEMINI.md"
Cohesion: 0.40
Nodes (4): Comandos clave, No hacer, Prioridad, Proyecto

### Community 116 - "Blazor - Neros"
Cohesion: 0.40
Nodes (4): After Editing, Blazor - Neros, Forms, UX Rules

### Community 117 - "Security - Neros"
Cohesion: 0.40
Nodes (4): Authorization, Input And Output, Review Triggers, Security - Neros

### Community 118 - "Crear Endpoint API - Neros"
Cohesion: 0.40
Nodes (4): Crear Endpoint API - Neros, Pasos, Reglas, Validacion

### Community 119 - "Crear Feature Blazor - Neros"
Cohesion: 0.40
Nodes (4): Crear Feature Blazor - Neros, Pasos, Reglas, Validacion

### Community 120 - "Crear Caso De Uso - Neros"
Cohesion: 0.40
Nodes (4): Crear Caso De Uso - Neros, Pasos, Reglas, Validacion

### Community 121 - "FilaEmpresa.razor"
Cohesion: 0.40
Nodes (4): AntiforgeryToken, Icono, IStringLocalizer<TextosEmpresas>, TextosEmpresas

### Community 122 - "PaginaNoEncontrada.razor"
Cohesion: 0.40
Nodes (4): IStringLocalizer<TextosSistema>, PageTitle, PaginaEstado, TextosSistema

### Community 123 - "PaginaEstado.razor"
Cohesion: 0.40
Nodes (4): Icono, Marca, Preferencias, TextosComunes

### Community 124 - "🧠 Diseñado para toda la organización"
Cohesion: 0.40
Nodes (5): 👔 Dirección, 🧠 Diseñado para toda la organización, 💼 Finanzas, 🏗️ Operaciones, 💻 Tecnología

### Community 125 - "WeatherForecast.cs"
Cohesion: 0.50
Nodes (3): Neros.Api, DateOnly, WeatherForecast

### Community 126 - "Antigravity Agents - Neros"
Cohesion: 0.50
Nodes (3): Antigravity Agents - Neros, Flujo, Reglas Compartidas

### Community 127 - "Codex Agents - Neros"
Cohesion: 0.50
Nodes (3): Codex Agents - Neros, Flujo, Fuente De Verdad

### Community 128 - "AGENTS.md"
Cohesion: 0.50
Nodes (3): Proyecto, Puentes Compatibles, Reglas

### Community 129 - "Base de datos Neros ERP"
Cohesion: 0.50
Nodes (4): Base de datos Neros ERP, Instalacion, Reversion, Seguridad y operacion

### Community 130 - "Architect Mode - Neros"
Cohesion: 0.50
Nodes (3): Architect Mode - Neros, Output, Workflow

### Community 131 - "Code Review Mode - Neros"
Cohesion: 0.50
Nodes (3): Code Review Mode - Neros, Output, Priorities

### Community 132 - "Debug Mode - Neros"
Cohesion: 0.50
Nodes (3): Debug Mode - Neros, Output, Workflow

### Community 133 - "Refactor Mode - Neros"
Cohesion: 0.50
Nodes (3): Output, Refactor Mode - Neros, Rules

### Community 134 - "ContextoEmpresa.razor"
Cohesion: 0.50
Nodes (3): Icono, IStringLocalizer<TextosInicio>, TextosInicio

### Community 135 - "AuthLayout.razor"
Cohesion: 0.50
Nodes (3): IStringLocalizer<TextosComunes>, LayoutComponentBase, TextosComunes

### Community 136 - "Aviso.razor"
Cohesion: 0.50
Nodes (3): Icono, IStringLocalizer<TextosComunes>, TextosComunes

### Community 137 - "SelectorTema.razor"
Cohesion: 0.50
Nodes (3): Icono, IStringLocalizer<TextosComunes>, TextosComunes

### Community 138 - "AGENTS.md"
Cohesion: 0.50
Nodes (3): Flujo recomendado, Puentes Compatibles, Reglas compartidas

### Community 139 - "⚡ Neros convierte procesos aislados en una sola operación conectada"
Cohesion: 0.50
Nodes (4): ⚙️ AUTOMATIZACIÓN, 🔗 CONEXIÓN, 🧭 CONTROL, ⚡ Neros convierte procesos aislados en una sola operación conectada

### Community 140 - "🌐 La visión a largo plazo"
Cohesion: 0.50
Nodes (4): 🌐 La visión a largo plazo, **NEROS**, Neros debe poder crecer junto con la empresa que lo utiliza., Operación conectada. Información confiable. Crecimiento sin fricción.

### Community 153 - "🏗️ Arquitectura"
Cohesion: 0.67
Nodes (3): 🏗️ Arquitectura, El dominio empresarial permanece en el centro., 🧩 Estructura de solución

### Community 154 - "🚀 Desarrollo local"
Cohesion: 0.67
Nodes (3): 🚀 Desarrollo local, Preparar, Requisitos

## Knowledge Gaps
- **833 isolated node(s):** `$schema`, `frontend-design@claude-plugins-official`, `feature-dev@claude-plugins-official`, `_comment`, `skillsPath` (+828 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **19 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Neros.Tests` connect `Neros.ServiceDefaults` to `AccesoMultiempresaTests`, `System.Text.Json`, `FoundationHttpTests.cs`, `System.Security.Claims`?**
  _High betweenness centrality (0.030) - this node is a cross-community bridge._
- **Why does `System.Security.Claims` connect `System.Security.Claims` to `Neros.ServiceDefaults`, `_Imports.razor`, `.TryFromAuthenticatedPrincipal`, `EndpointsSesion.cs`?**
  _High betweenness centrality (0.025) - this node is a cross-community bridge._
- **Why does `Neros.ServiceDefaults` connect `Neros.ServiceDefaults` to `FoundationHttp`, `.TryFromAuthenticatedPrincipal`, `System.Security.Claims`, `FoundationHttpTests.cs`, `EndpointsSesion.cs`, `Microsoft.AspNetCore.Authorization`, `ServiceAuthentication.cs`?**
  _High betweenness centrality (0.024) - this node is a cross-community bridge._
- **What connects `$schema`, `frontend-design@claude-plugins-official`, `feature-dev@claude-plugins-official` to the rest of the system?**
  _833 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Neros.Tests.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.05117845117845118 - nodes in this community are weakly interconnected._
- **Should `AccesoMultiempresaTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09990749306197964 - nodes in this community are weakly interconnected._
- **Should `FoundationHttp` be split into smaller, more focused modules?**
  _Cohesion score 0.05813953488372093 - nodes in this community are weakly interconnected._