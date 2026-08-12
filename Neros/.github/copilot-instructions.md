# GitHub Copilot - Neros

Guia principal para trabajar dentro de `Neros`. La fuente de verdad completa es `../AGENTS.md` y las convenciones funcionales estan en `../CONVENTIONS.md`.

## Proyecto

- Nueva migracion de Neros a .NET 10 por capas.
- Frontend: Blazor Web App con Tailwind CSS.
- Backend HTTP: ASP.NET Core Web API.
- Core: `Shared`, `Domain`, `Contracts`, `Application`.
- Seguridad objetivo: Identity, autorizacion por rol/recurso y auditoria.
- Comunicacion en tiempo real objetivo: SignalR con payloads pequenos y contratos explicitos.
- Graphify: usar `graphify-out/graph.json` para consultas de arquitectura cuando se pueda ejecutar local.

## Reglas duras

- No Bootstrap, jQuery, DataTables ni Select2 en el frontend nuevo.
- No acceso a datos desde Blazor.
- Domain permanece puro.
- Application orquesta casos de uso y define puertos.
- API delega en Application; los controladores no contienen reglas de negocio extensas.
- Contracts contiene DTOs compartidos por Blazor/API; no exponer entidades internas.
- Scripts SQL Server manuales para cambios de base de datos cuando aplique.
- No secretos en archivos versionados.
- No copiar arquitectura MVC Areas/Servicios del sistema anterior salvo solicitud explicita de analisis legacy.

## Validacion

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

Si el usuario prohibe ejecutar comandos locales, usa diagnosticos del editor y deja estos comandos como verificacion pendiente.

