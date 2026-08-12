# GitHub Copilot - Neros

Este workspace contiene `Neros`, la nueva migracion full .NET por capas. El codigo viejo solo sirve como referencia funcional; no copies su arquitectura si contradice el modelo nuevo.

## Fuente de verdad

- Para cualquier cambio dentro del proyecto nuevo, leer y seguir `Neros/AGENTS.md`.
- Convenciones funcionales: `Neros/CONVENTIONS.md`.
- Skills locales: `Neros/skills/*/SKILL.md`.
- Grafo de arquitectura: `Neros/graphify-out/graph.json` cuando exista.

## Identidad del proyecto

- Nombre correcto: `Neros`.
- No usar `Neros.Next`, `Aurosoft.Next` ni nombres del monolito como arquitectura objetivo.
- Stack objetivo: .NET 10, Blazor Web App, ASP.NET Core Web API, Clean Architecture, Tailwind CSS, SQL Server, Identity, SignalR, jobs y Graphify.

## Reglas duras

- Mantener separacion por capas: Blazor -> Api -> Application -> Domain; `Contracts` y `Shared` son contratos/tipos transversales.
- `Neros.Blazor` no accede a datos ni referencia persistencia; consume API/servicios tipados y DTOs de `Neros.Contracts`.
- `Neros.Api` expone endpoints y delega casos de uso en `Neros.Application`.
- `Neros.Application` define casos de uso, puertos, validaciones y transacciones; no implementa SQL, HTTP externo ni filesystem.
- `Neros.Domain` permanece puro: entidades, value objects, reglas y eventos de dominio sin ASP.NET, EF Core ni UI.
- Usar Tailwind CSS; no Bootstrap, jQuery, DataTables ni Select2.
- No guardar secretos. Usar User Secrets, variables de entorno o secret manager.
- Base de datos: preferir scripts SQL Server manuales versionados en `database/scripts/`; EF Core mapea y consulta, no decide el esquema por si solo.

## Workflow de agentes

- Para arquitectura o impacto entre proyectos, consultar Graphify antes de decidir.
- Para features: plan pequeno, tocar la capa propietaria, validar con el comando mas estrecho disponible.
- Para UI: compilar Tailwind antes del build .NET cuando el usuario permita ejecutar comandos.
- Si el usuario pide no ejecutar local, trabajar solo con lectura/edicion/diagnosticos del editor y dejar comandos sugeridos.

## Comandos canonicos

```powershell
cd .\Neros
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

