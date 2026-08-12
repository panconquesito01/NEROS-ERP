# GEMINI.md

Instrucciones para Gemini CLI / agentes compatibles en `Neros`.

## Prioridad

- Usar `AGENTS.md` como guia principal.
- Aplicar `CONVENTIONS.md` para nombres, capas, UI y base de datos.
- Tratar el monolito anterior solo como referencia funcional de migracion.

## Proyecto

- .NET 10, Blazor Web App, ASP.NET Core Web API, Clean Architecture y Tailwind CSS.
- Capas: `Neros.Blazor`, `Neros.Api`, `Neros.Application`, `Neros.Domain`, `Neros.Contracts`, `Neros.Shared`.
- Persistencia futura por puertos de Application; SQL Server con scripts manuales versionados.
- Seguridad objetivo con Identity, autorizacion por recurso y auditoria.
- SignalR para eventos operativos en tiempo real con contratos explicitos.

## No hacer

- No usar `Neros.Next`, Aurosoft/Equaltech ni MVC Areas como arquitectura objetivo.
- No introducir Bootstrap, jQuery, DataTables ni Select2.
- No poner reglas de negocio en Blazor o controladores API.
- No exponer secretos ni entidades internas en DTOs publicos.

## Comandos clave

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify query "estructura actual de Neros"
```

