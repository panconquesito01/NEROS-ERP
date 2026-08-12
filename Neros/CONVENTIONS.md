# CONVENTIONS.md

Convenciones de `Neros` para codigo, UI, datos, seguridad y agentes.

## Idioma Y Nombres

- Idioma preferido: espanol para nombres de negocio, UI y documentacion funcional.
- C#: PascalCase para tipos y miembros publicos; camelCase para variables locales y campos privados sin prefijo salvo estilo local existente.
- Usar nombres de casos de uso orientados a accion de negocio: `CrearPedido`, `ProcesarReempaque`, `ConsultarSaldos`.
- Evitar abreviaturas si no son terminos del dominio.

## Capas

- `Neros.Shared`: tipos transversales simples y sin dependencias pesadas.
- `Neros.Domain`: entidades, value objects, reglas puras e invariantes.
- `Neros.Contracts`: DTOs, requests, responses y contratos entre UI/API.
- `Neros.Application`: casos de uso, puertos, validaciones, politicas y transacciones.
- `Neros.Api`: endpoints HTTP, autenticacion, autorizacion, DI y adaptadores de borde.
- `Neros.Blazor`: UI, estado visual, formularios, navegacion y consumo de servicios/API.

## Dependencias Permitidas

```text
Blazor -> Contracts / Shared / API clients
Api -> Application / Contracts / Shared
Application -> Domain / Contracts / Shared
Domain -> sin infraestructura
Infrastructure/Persistence futura -> Application ports
```

## No Hacer

- No poner logica de negocio en componentes Blazor o controladores API.
- No acceder a datos desde Blazor.
- No exponer entidades internas como contratos HTTP.
- No copiar MVC Areas, `Servicios/*`, Bootstrap o jQuery del sistema anterior como arquitectura objetivo.
- No guardar secretos, tokens o connection strings en archivos versionados.

## Frontend

- Tailwind CSS es el sistema visual base.
- No Bootstrap, jQuery, DataTables ni Select2.
- Componentes Blazor pequenos, con estado claro y servicios tipados.
- Formularios con validacion visible, loading/error/empty states y acciones deshabilitadas durante guardado.
- UX de ERP: densa, escaneable, accesible y orientada a trabajo repetido.

## API Y Application

- Endpoints/controladores delgados: validar, autorizar, llamar Application, mapear respuesta.
- Application orquesta reglas, permisos de negocio, puertos y transacciones.
- Domain valida invariantes que no dependen de infraestructura.
- Usar `CancellationToken` en operaciones I/O y casos de uso largos.

## Base De Datos

- Persistencia futura como infraestructura que implementa puertos de Application.
- SQL Server como motor objetivo.
- Cambios de esquema con scripts manuales en `database/scripts/`.
- EF Core puede mapear y consultar, pero no es la unica fuente de verdad del esquema.
- SQL raw siempre parametrizado.

## Seguridad

- Identity sera la base de autenticacion.
- Autorizacion server-side por rol, recurso, empresa/tenant/sucursal cuando aplique.
- Auditoria para acciones sensibles: login, permisos, procesamiento, cierres, anulaciones y reportes.
- No loggear secretos ni payloads sensibles.

## Validacion

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

Si el usuario prohibe ejecucion local, usar diagnosticos del editor y dejar estos comandos pendientes.