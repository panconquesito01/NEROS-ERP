# CONVENTIONS.md

Convenciones de `Neros` para codigo, UI, datos, seguridad y agentes.

## Idioma Y Nombres

- Idioma preferido: espanol para nombres de negocio, UI y documentacion funcional.
- C#: PascalCase para tipos y miembros publicos; camelCase para variables locales y campos privados sin prefijo salvo estilo local existente.
- Usar nombres de casos de uso orientados a accion de negocio: `CrearPedido`, `ProcesarReempaque`, `ConsultarSaldos`.
- Evitar abreviaturas si no son terminos del dominio.

## Capas

Arquitectura vigente: [ADR-0002](docs/adr/ADR-0002-distributed-modular-platform.md). Las capas globales siguientes son compatibilidad, no librerias empresariales para servicios nuevos. Cada bounded context posee sus capas/contratos/base/scripts; no SQL, FK, DbContext ni referencias a reglas de otro servicio. Integrar por APIs/eventos/proyecciones autorizadas y transacciones locales con Outbox/Inbox. Solo tecnicos en BuildingBlocks.

- `Neros.Shared`: tipos transversales simples y sin dependencias pesadas.
- `Neros.Domain`: entidades, value objects, reglas puras e invariantes (p. ej. `Globalizacion/Dinero`, `MotorRedondeo`, `Impuestos/MotorImpuestos`, `Contabilidad/MotorPartidaDoble`, `Inventario/MotorPromedioPonderado`).
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

### Estructura De Neros.Blazor

```text
Neros.Blazor/
|-- Components/
|   |-- App.razor, Routes.razor, _Imports.razor
|   |-- Layout/            # Estructura autenticada y de acceso: MainLayout, AuthLayout, NavMenu, ReconnectModal
|   |-- Shared/            # Piezas reutilizables sin conocimiento de un modulo: Icono, Marca, Aviso, EstadoVacio,
|   |                      # PaginaEstado, Preferencias (SelectorIdioma + SelectorTema), TextosCliente,
|   |                      # Presentacion (formato de UI), CatalogoModulos
|   `-- Features/          # Una carpeta por modulo funcional, alineada con su bounded context
|       |-- Acceso/        # PaginaAcceso + componentes propios (CatalogoAcceso) + TextosAcceso(.resx)
|       |-- Empresas/      # PaginaEmpresas + FilaEmpresa + TextosEmpresas(.resx)
|       |-- Inicio/        # PaginaInicio + ContextoEmpresa, ActividadReciente, ModulosEnPreparacion + TextosInicio(.resx)
|       `-- Sistema/       # PaginaNoEncontrada, PaginaError + TextosSistema(.resx)
|-- Localizacion/          # Idiomas disponibles, configuracion de cultura, endpoint /idioma y TextosComunes(.resx)
|-- Servicios/             # Clientes HTTP tipados hacia Gateway/API y endpoints BFF de sesion
|-- Styles/tailwind.css    # Fuente de estilos; wwwroot/app.css es generado
`-- wwwroot/               # Scripts de mejora progresiva, iconos locales y marca
```

- Un modulo nuevo (Ventas, Inventario...) crea `Features/<Modulo>/` con su `Pagina<Nombre>.razor` y sus subcomponentes; no agrega paginas sueltas fuera de `Features`.
- Las paginas enrutables usan prefijo `Pagina` y solo orquestan: cargan datos del cliente tipado, resuelven estados (carga, error, vacio) y componen subcomponentes que reciben DTOs por parametro.
- Un componente pasa a `Shared/` solo cuando lo usan dos o mas modulos y no depende de reglas de ninguno. `Features/<A>` no referencia componentes de `Features/<B>`.
- Estados de error y vacio usan `Aviso` y `EstadoVacio`; paginas de sistema usan `PaginaEstado`. No duplicar su markup.
- La disponibilidad de modulos se declara solo en `CatalogoModulos`; acceso y inicio la leen de ahi.
- Cada modulo nuevo tendra su cliente tipado propio en `Servicios/` (por ejemplo `ClienteVentas`) cuando exista su API; no crecer `ClienteNeros` con endpoints de otros contextos.

### Idiomas (i18n)

- Ningun texto visible se escribe literal en `.razor` ni en `wwwroot/*.js`. Se usa `IStringLocalizer<T>` con archivos `.resx`; el codigo, las claves y el idioma base siguen en espanol.
- Cada modulo es dueno de sus textos: `Features/<Modulo>/Textos<Modulo>.cs` (clase marcadora vacia) junto a `Textos<Modulo>.resx` (espanol, base), `.en.resx` y `.pt.resx`. En el componente: `@inject IStringLocalizer<Textos<Modulo>> T`.
- `Localizacion/TextosComunes` guarda solo lo compartido (layout, navegacion, tema, idioma, catalogo de modulos, reconexion) y se inyecta como `C`. Un texto sube a comunes con la misma regla que un componente sube a `Shared/`.
- Claves en PascalCase con puntos para agrupar (`Mensaje.credenciales`, `Modulo.Ventas.Nombre`). Valores con variables usan `{0}`: `T["Saludo", nombre]`. Plural simple con dos claves (`ContadorUno`/`ContadorVarios`).
- Textos para JavaScript: clave `Js.<Nombre>` en `TextosComunes`. `TextosCliente` los publica como JSON y los scripts los leen con `window.nerosTexto('Nombre', ...valores)`. Fechas y numeros en JS usan `document.documentElement.lang` con `Intl`.
- Datos que vienen de la base (nombres de empresa, roles, acciones de auditoria) no se traducen en la UI salvo que exista clave explicita; por ejemplo `Accion.<valor>` en `TextosInicio`, con respaldo al valor original.
- La cultura se resuelve por cookie `.AspNetCore.Culture` (la fija `POST /idioma`, con antiforgery), luego `Accept-Language` del navegador, luego espanol.
- Agregar un idioma: sumarlo a `Localizacion/Idiomas.Disponibles` con su nombre nativo y crear `<Recurso>.<codigo>.resx` para cada recurso con las mismas claves que el `.resx` base. Sin traduccion completa, .NET cae al espanol clave por clave.

## API Y Application

- Endpoints/controladores delgados: validar, autorizar, llamar Application, mapear respuesta.
- Application orquesta reglas, permisos de negocio, puertos y transacciones.
- Domain valida invariantes que no dependen de infraestructura.
- Usar `CancellationToken` en operaciones I/O y casos de uso largos.

## Base De Datos

- Tenant -> BusinessGroup -> Company -> Branch; una empresa pertenece a un tenant contractual. No DefaultTenant, TenantId nullable en tablas nuevas de negocio ni CompanyId usado como tenant. Indices unicos y autorizacion consideran el ambito correcto.
- Migraciones con Company -> Tenant explicito, sin asignacion silenciosa; grupos nunca cruzan tenants. Shared/sharded/dedicated usan mismo modelo funcional y contratos, no forks.

- Persistencia futura como infraestructura que implementa puertos de Application.
- SQL Server como motor objetivo.
- Esquema solo con scripts versionados e inmutables en `database/<modulo>/migrations`, aplicados con `tools/Neros.Database.Deploy`. Reglas completas en [SQL_CONVENTIONS](database/conventions/SQL_CONVENTIONS.md) y [DEPLOYMENT_GUIDE](database/conventions/DEPLOYMENT_GUIDE.md).
- EF Core mapea y consulta; nunca crea ni cambia esquema (sin Migrations, `Migrate()`, `EnsureCreated()` ni `GenerateCreateScript()`). La prueba de deriva compara el modelo EF con la base desplegada por scripts.
- SQL raw siempre parametrizado.

## Seguridad

- Decision vigente del usuario: AdministradorGlobal accede a todos los tenants mediante privilegio de plataforma explicito y auditado, seleccionando un tenant por operacion. No quitar filtros ni omitir validacion de identidad/recurso. Usuarios normales pueden tener membresias independientes por tenant.
- Mantener revocacion vigente/fail closed en operaciones sensibles; un JWT sin decision actual no acredita membresia vigente. Eventos/jobs/archivos/caches/Search/Analytics conservan TenantId y autorizacion por propietario.

- Identity sera la base de autenticacion.
- Autorizacion server-side por rol, recurso, empresa/tenant/sucursal cuando aplique.
- Permisos con formato `MODULO.RECURSO.ACCION`: el codigo se declara en `Neros.Contracts.Seguridad.CodigosPermiso` y la asignacion en `Neros.Application.Seguridad.Permisos`. Cada permiso es una politica de la API (`[Authorize(Policy = ...)]`); el BFF solo lo usa para mostrar u ocultar opciones.
- Auditoria para acciones sensibles: login, permisos, procesamiento, cierres, anulaciones y reportes. Registrar actor y entidad afectada en `auditoria.Evento` (append-only) y nunca claves, tokens ni secretos en `Detalle`.
- No loggear secretos ni payloads sensibles.

## Validacion

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

Si el usuario prohibe ejecucion local, usar diagnosticos del editor y dejar estos comandos pendientes.