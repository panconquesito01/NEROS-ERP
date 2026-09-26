# Base de datos Neros ERP

El esquema se administra solo con scripts SQL versionados por módulo, aplicados con `tools/Neros.Database.Deploy` ([ADR-0004](../docs/adr/ADR-0004-plan-maestro-scripts-sql-y-salto-de-gates.md)). No hay EF Migrations, `EnsureCreated()` ni generación del esquema desde EF, y la API no aplica cambios al iniciar.

| Carpeta | Contenido |
|---|---|
| [`conventions/`](conventions/) | [Convenciones SQL](conventions/SQL_CONVENTIONS.md) y [guía de despliegue](conventions/DEPLOYMENT_GUIDE.md) |
| [`compatibilidad/`](compatibilidad/) | Identity, empresas, membresías, sesiones y auditoría de acceso del host `Neros.Api` (base `NEROSERP`) |
| [`privacidad/`](privacidad/) | Catálogo de datos personales, documentos legales versionados, aceptaciones, cookies y políticas de retención (esquema `privacidad` en la misma base) |
| [`organizacion/`](organizacion/) | Tenant, grupo, empresa (correspondencia con compatibilidad), sucursal y configuración regional (base `NEROSORG`) |
| [`globalizacion/`](globalizacion/) | Catálogos **transversales** (sin `EmpresaId`): ISO, zonas horarias, unidades, tipos de identificación, redondeo, tasas y registro normativo (`NEROSGLOBAL` o base dedicada). API: `GET api/globalizacion/catalogos` |
| [`terceros/`](terceros/) | Maestro de identidad, identificaciones, roles y cuentas bancarias de proveedor con historial temporal (`NEROSTERCEROS`) |
| [`impuestos/`](impuestos/) | Configuracion tributaria y versiones publicadas inmutables (`NEROSIMPUESTOS`) |
| [`contabilidad/`](contabilidad/) | Plan de cuentas, periodos, comprobantes, reportes configurables V0003 (`NEROSCONT`) |
| [`inventario/`](inventario/) | Bodegas, movimientos inmutables y existencias (`NEROSINV`) |
| [`ventas/`](ventas/) | Cotizacion y pedido con snapshot de tercero e impuestos (`NEROSVENTAS`) |
| [`compras/`](compras/) | Orden de compra, recepciones y acumulado por linea (`NEROSCOMPRAS`) |
| [`integracion/`](integracion/) | Documentacion outbox/inbox por modulo y broker local (fase 13) |
| [`cartera/`](cartera/) | CxC, CxP, cuotas, pagos y aplicaciones (`NEROSCARTERA`) |
| [`tesoreria/`](tesoreria/) | Caja, banco, movimientos y conciliacion (`NEROSTESORERIA`) |
| [`facturacion/`](facturacion/) | Facturacion electronica Colombia: comercial, fiscal, electronico y numeracion (`NEROSFACTURACION`) |
| [`nomina/`](nomina/) | Empleados segregados, contratos, conceptos, periodos y liquidacion con snapshot (`NEROSNOMINA`) |
| [`activos/`](activos/) | Activos fijos, depreciacion lineal, deterioro y baja (`NEROSACTIVOS`) |
| [`presupuesto/`](presupuesto/) | Version, centro de costo, lineas y ejecucion vs real (`NEROSPRESUPUESTO`) |
| [`produccion/`](produccion/) | BOM, ruta, orden, movimientos y costos (`NEROSPRODUCCION`) |
| [`proyectos/`](proyectos/) | Proyecto, fases, presupuesto e imputacion de movimientos (`NEROSPROYECTOS`) |
| [`analitica/`](analitica/) | Warehouse D-17a: ingesta por eventos, hechos e indicadores (`NEROSANALYTICS`) |
| [`busqueda/`](busqueda/) | Indice D-17b: ACL, tombstones, vista 360 y frescura (`NEROSSEARCH`) |

## Instalación nueva

Requiere SQL Server en localhost y una cuenta Windows con permiso para crear bases. La cuenta que ejecuta la API solo necesita lectura y escritura, no permisos de administrador del servidor.

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo compatibilidad --servidor localhost --base NEROSERP --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo privacidad --servidor localhost --base NEROSERP
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo organizacion --servidor localhost --base NEROSORG --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo globalizacion --servidor localhost --base NEROSGLOBAL --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo terceros --servidor localhost --base NEROSTERCEROS --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo impuestos --servidor localhost --base NEROSIMPUESTOS --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo contabilidad --servidor localhost --base NEROSCONT --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo inventario --servidor localhost --base NEROSINV --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo ventas --servidor localhost --base NEROSVENTAS --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo compras --servidor localhost --base NEROSCOMPRAS --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo cartera --servidor localhost --base NEROSCARTERA --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo tesoreria --servidor localhost --base NEROSTESORERIA --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo facturacion --servidor localhost --base NEROSFACTURACION --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo nomina --servidor localhost --base NEROSNOMINA --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo activos --servidor localhost --base NEROSACTIVOS --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo presupuesto --servidor localhost --base NEROSPRESUPUESTO --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo produccion --servidor localhost --base NEROSPRODUCCION --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo proyectos --servidor localhost --base NEROSPROYECTOS --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo analitica --servidor localhost --base NEROSANALYTICS --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo busqueda --servidor localhost --base NEROSSEARCH --crear-base
dotnet run --project .\Neros.Api --launch-profile http -- --inicializar-admin
```

`apply` crea la base si falta, aplica los scripts pendientes en transacción, los registra en `dbo.NerosSchemaVersion` y ejecuta las validaciones. Crear el administrador con el comando local de la [documentación técnica](../docs/tecnica/acceso-multiempresa.md). No insertar claves ni hashes manuales en SQL.

## Base existente creada con los scripts anteriores

Las bases instaladas con el antiguo `database/scripts/001_identity_empresas.sql` ya tienen el esquema de `V0001`. Se adoptan sin reejecutarlo:

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- baseline --modulo compatibilidad --servidor localhost --base NEROSERP --hasta V0001
dotnet run --project .\tools\Neros.Database.Deploy -- validate --modulo compatibilidad --servidor localhost --base NEROSERP
```

A partir de ahí, `plan` muestra lo pendiente y `apply` lo aplica. Tras el baseline queda pendiente `V0002` (cuenta, sesiones y auditoría), que la versión actual de la API necesita:

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- plan --modulo compatibilidad --servidor localhost --base NEROSERP
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo compatibilidad --servidor localhost --base NEROSERP
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo privacidad --servidor localhost --base NEROSERP
```

El módulo `privacidad` depende del esquema base de `compatibilidad` (misma base `NEROSERP`); en instalaciones nuevas aplicar compatibilidad antes que privacidad.

## Cambios de esquema

Crear el siguiente `V<NNNN>__<descripcion>.sql` del módulo con el encabezado obligatorio. Un script aplicado no se edita: el runner detecta el cambio de checksum y se detiene. Ver [convenciones](conventions/SQL_CONVENTIONS.md).

## Seguridad y operación

- La cadena local reside en User Secrets de la API bajo `ConnectionStrings:Neros`, no en estos archivos. El runner usa seguridad integrada o `--conexion-variable`.
- La configuración local acepta el certificado del servidor; en producción usar un certificado confiable.
- El rol pertenece a `UsuariosEmpresas`, nunca al usuario global.
- Desactivar una membresía impide inmediatamente las siguientes consultas a esa empresa.
- Desactivar un usuario o cambiar su sello de seguridad invalida sus sesiones.
- No borrar auditoría para corregir errores operativos. `auditoria.Evento` es un ledger append-only y SQL Server rechaza UPDATE y DELETE.
- Sesiones vencidas o revocadas: definir un job administrado con retención (por ejemplo 90 días) que borre `dbo.Sesiones` con `Expira` o `RevocadaEnUtc` anteriores a la retención.
- Definir una política de retención y archivo para auditoría antes de producción.

## Reversión

Cada script transaccional se revierte completo si falla, y el intento queda registrado con su error. Para cambios ya aplicados se prefiere roll forward con un script nuevo; si hay datos, tomar un backup antes. Ver [guía de despliegue](conventions/DEPLOYMENT_GUIDE.md).
