# PLAN MAESTRO INTEGRAL — NEROS ERP

**Versión:** 2.2 (cierre de implementación base)
**Fecha:** 2026-09-26
**Estado:** APROBADO (2026-09-25) e **implementación base de fases 0–26 completada** — seguimiento en [NEROS_EXECUTION_STATUS.md](NEROS_EXECUTION_STATUS.md) y [ADR-0004](../adr/ADR-0004-plan-maestro-scripts-sql-y-salto-de-gates.md)
**Ámbito regulatorio inicial:** Colombia
**Arquitectura:** plataforma distribuida por módulos ([ADR-0002](../adr/ADR-0002-distributed-modular-platform.md)), preparada para múltiples jurisdicciones
**Base de datos:** SQL Server, una base por módulo, esquema administrado solo con scripts SQL versionados
**ORM:** EF Core permitido para acceso a datos; **EF Migrations, `EnsureCreated()` y `GenerateCreateScript()` prohibidos para crear o cambiar esquema**
**Reemplaza a:** `PLAN_CUENTA_DASHBOARD_MODULOS.md` (sus decisiones quedan integradas en §0.2, Parte III y Parte VIII)

---

# 0. Control de cambios

## 0.1. Correcciones respecto a la versión 2.0

| # | Problema en v2.0 | Corrección en v2.1 |
|---|---|---|
| 1 | Scripts por tipo de objeto (`002_tables.sql`, `004_indexes.sql`...) contradicen la regla "un script aplicado no se modifica": agregar una tabla obligaría a editar `002_tables.sql` | Scripts **secuenciales e inmutables** por módulo (`V0001__...sql`); el tipo de objeto va en el nombre, no en el archivo (§4) |
| 2 | Prefijo `CO_` para Contabilidad choca con el código ISO de Colombia (`CO`) | **Esquemas SQL por módulo** (`contabilidad.Comprobante`), sin prefijos (§5) |
| 3 | FK entre tablas de módulos distintos implícitas en convenciones y ejemplos | FK solo **dentro** de la base de un módulo; entre módulos, IDs + snapshot + eventos (ADR-0002) (§3) |
| 4 | "Activo = Pasivo + Patrimonio" como invariante de prueba falla antes del cierre | Invariante correcta: `Activo = Pasivo + Patrimonio + (Ingresos − Costos − Gastos)` antes del cierre (§33) |
| 5 | Fórmula de saldo CxC/CxP cuenta dos veces los pagos (`− Créditos − PagosAplicados`) | `Saldo = Σ cargos − Σ abonos`, con los pagos como un tipo de abono (§43) |
| 6 | Diferencia en cambio sin sentido del efecto ni realizada/no realizada | Signo según CxC o CxP, pagos parciales y ajuste no realizado al cierre (§45) |
| 7 | Impuesto incluido `Total / (1 + Tarifa)` presentado como general | Válido solo para un impuesto porcentual único; con varios impuestos o impuestos en cascada se resuelve por el motor (§39) |
| 8 | Costo promedio sin tratar movimientos retroactivos ni existencias negativas | Política de recosteo cronológico, bloqueo por cierre de inventario y regla de negativos (§49) |
| 9 | Numeración: se prohíbe `MAX+1` pero no se aborda que `SEQUENCE` e `IDENTITY` dejan huecos | Número fiscal asignado **al emitir**, en la misma transacción, con bloqueo de fila del rango; nunca en borrador (§52) |
| 10 | Fechas: solo UTC | Distinción entre **instante** (`datetime2` UTC) y **fecha de negocio** (`date` en la zona de la empresa) (§12) |
| 11 | `decimal(19,4)` para todo importe, incluso costo unitario y precio unitario | Precisión diferenciada: unitarios y tasas con más decimales (§11) |
| 12 | Multimoneda sin doble importe en movimientos | Cada movimiento guarda importe en moneda del documento y en moneda funcional con la tasa usada (§31) |
| 13 | Terceros con rol `EMPLEADO` y cuenta bancaria en el mismo maestro que todos leen | Terceros solo guarda identidad; salario, contrato y banco del empleado viven segregados en Nómina (§36) |
| 14 | Impuestos como dominio sin decidir cómo lo consumen Ventas y Compras | Impuestos es dueño de la configuración y publica **versiones inmutables**; Ventas y Compras calculan en proceso con un motor puro y guardan la versión usada (§38) |
| 15 | Banner de cookies obligatorio aunque hoy solo hay cookies esenciales | `/cookies` y catálogo siempre; banner solo cuando exista una cookie no esencial (§25) |
| 16 | Auditoría inmutable sin mecanismo | Tablas append-only con `DENY UPDATE, DELETE`, y **tablas ledger** de SQL Server para auditoría y comprobantes contabilizados (§14) |
| 17 | Roadmap de 26 fases sin relación con D-01..D-18 ni con el bloqueo de D-03 | Roadmap reconciliado con los D-XX, y el broker (D-03) como ruta crítica de toda integración (Parte X) |
| 18 | Se omiten las peticiones ya acordadas: menú de cuenta, cambio de clave, restablecimiento por administrador, dashboard, i18n | Integradas en Fases 2 y 5 (Parte III y VIII) |
| 19 | Pruebas actuales usan `EnsureCreated()` y la API tiene `--generar-sql` | Migrar pruebas al despliegue por scripts y retirar `--generar-sql` (§8) |
| 20 | Referencias normativas citadas como hechos verificados | Toda norma entra al registro normativo con estado **POR VERIFICAR** hasta revisión de especialista (§29) |

## 0.2. Decisiones del usuario ya tomadas (2026-09-25)

| Tema | Decisión |
|---|---|
| Restablecer clave de otros usuarios | Solo `AdministradorGlobal`, validado en servidor |
| Método | Clave temporal mostrada una sola vez y cambio obligatorio en el siguiente acceso; sin correo |
| Acciones de cuenta de cualquier usuario | Cambiar su clave, cambiar de empresa, cerrar sesión, idioma y tema desde un menú de usuario |
| País, moneda y zona horaria de la empresa | Esperan al modelo Tenant/Organización (Fase 4); antes solo tipos base |
| Backend de módulos | Autorizado saltar gates del roadmap D-XX, documentándolo en ADR-0004 |
| Terceros | Módulo base compartido |

---

# PARTE I — PROPÓSITO Y PRINCIPIOS

# 1. Propósito

Neros no se construye como una colección de CRUD. Es una plataforma ERP que durante años debe soportar múltiples tenants, grupos, empresas, sucursales, usuarios, monedas, países y ejercicios, con integridad financiera, auditoría, cumplimiento y procesos asíncronos, sin perder trazabilidad legal.

Toda funcionalidad se evalúa en cinco dimensiones inseparables y no está terminada si falla en una:

```text
DOMINIO · DATOS · SEGURIDAD · EXPERIENCIA · CUMPLIMIENTO
```

# 2. Principios no negociables

1. Los cálculos financieros autoritativos se ejecutan en backend; el frontend solo previsualiza.
2. La base de datos protege su integridad (PK, FK internas, CHECK, UNIQUE) sin depender del frontend.
3. Ninguna regla legal es una constante dispersa: vive en configuración versionada con vigencia desde/hasta y referencia normativa.
4. Los documentos históricos conservan la configuración, las tarifas y los datos de terceros usados al emitirse (snapshot).
5. El esquema cambia solo mediante scripts SQL explícitos, revisables, versionados e inmutables una vez aplicados.
6. Todo movimiento financiero se rastrea hasta su documento origen, y viceversa.
7. Lo contabilizado no se edita ni se elimina: se reversa y se corrige con trazabilidad.
8. `TenantId` y `EmpresaId` son obligatorios en información empresarial y `TenantId ≠ EmpresaId`.
9. Seguridad y autorización siempre en servidor; fail closed cuando no se puede confirmar.
10. La auditoría no puede deshabilitarse ni editarse por usuarios operativos.
11. Procesos masivos van a jobs, no a solicitudes HTTP.
12. Cada módulo es dueño de su base; ningún módulo lee ni escribe la base de otro.
13. Todo resultado derivado (existencias, saldos) debe poder reconstruirse desde movimientos.
14. Toda función legalmente sensible requiere validación de especialista antes de declararse apta para producción.
15. Rendimiento, Design System e i18n (es/en/pt) son requisitos, no mejoras.

---

# PARTE II — ARQUITECTURA Y DATOS

# 3. Topología de datos

- **Una base por módulo** (`NerosIdentidad`, `NerosOrganizacion`, `NerosTerceros`, `NerosInventario`, `NerosVentas`, `NerosCompras`, `NerosContabilidad`, `NerosImpuestos`, `NerosNomina`, `NerosTesoreria`...), con su propio usuario de aplicación de mínimo privilegio.
- FK solo entre tablas de la misma base. Entre módulos se guarda el ID externo más un snapshot de los datos necesarios, y la coherencia se mantiene por eventos.
- Reportes que cruzan módulos se construyen con proyecciones alimentadas por eventos (D-17), nunca con joins entre bases.
- En desarrollo pueden convivir en una instancia; en producción el despliegue decide (shared/sharded/dedicated) sin cambiar código.
- La base de compatibilidad actual (`NEROSERP`: identidad, empresas, sesiones) se mantiene hasta su extracción en Fases 2 y 4.

# 4. Scripts SQL como fuente de verdad

## 4.1. Estructura

```text
database/
├── README.md
├── conventions/                # SQL_CONVENTIONS (nombres, tipos, índices, encabezado) y DEPLOYMENT_GUIDE
│                               # (el runner vive en tools/Neros.Database.Deploy)
├── compatibilidad/             # NEROSERP actual; baseline de 000/001 existentes
├── identidad/
├── organizacion/
├── terceros/
├── impuestos/
├── contabilidad/
├── inventario/
├── ventas/
├── compras/
├── tesoreria/
├── nomina/
├── privacidad/
└── <modulo>/
    ├── migrations/             # V0001__crear_esquema.sql, V0002__crear_tabla_cuenta.sql ...
    ├── repeatable/             # R__vista_saldos.sql, R__permisos.sql (se reaplican si cambia su checksum)
    ├── seed/                   # Datos de referencia idempotentes (catálogos), nunca datos de negocio
    ├── validation/             # Consultas que deben devolver 0 filas de error tras desplegar
    ├── README.md
    └── CHANGELOG.md
```

## 4.2. Reglas

- `migrations/` es secuencial e inmutable: un archivo aplicado nunca se modifica. Una corrección es un nuevo `V00NN`.
- `repeatable/` contiene solo objetos que se pueden recrear sin perder datos (vistas, funciones, procedimientos, permisos) y se reaplican cuando cambia su checksum.
- `seed/` usa `MERGE` o `IF NOT EXISTS`, idempotente y con catálogos, nunca con datos de clientes.
- `validation/` se ejecuta después de cada despliegue; cualquier fila devuelta hace fallar el despliegue.
- Los scripts existentes `000_crear_base.sql` y `001_identity_empresas.sql` se adoptan como baseline `V0001` de `compatibilidad`, registrados sin reejecutarse en bases que ya los tienen.

# 5. Convenciones SQL

- Esquema SQL por módulo: `contabilidad.Comprobante`, `inventario.Movimiento`. Sin prefijos de tabla.
- Nombres en PascalCase y en español, singular.

| Objeto | Convención |
|---|---|
| PK | `PK_<Tabla>` |
| FK | `FK_<Tabla>_<TablaDestino>_<Campo>` |
| Índice | `IX_<Tabla>_<Campos>` |
| Único | `UX_<Tabla>_<Campos>` |
| Check | `CK_<Tabla>_<Regla>` |
| Default | `DF_<Tabla>_<Campo>` |

- Toda tabla de negocio: `Id` (uniqueidentifier secuencial generado por la aplicación o bigint según volumen), `TenantId`, `EmpresaId`, `CreadoPor`, `CreadoEnUtc`, `ModificadoPor`, `ModificadoEnUtc`, `Version rowversion`.
- Índices que empiezan por `TenantId, EmpresaId` cuando la consulta filtra por ellos.
- Toda FK interna con índice de soporte, salvo justificación.
- Prohibido `SELECT *`, `NOLOCK` por costumbre y SQL concatenado.

# 6. Encabezado obligatorio de cada script

```sql
/*
===============================================================================
Neros ERP
Script        : V0004__indices_movimiento.sql
Modulo        : Contabilidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Indices para consultas de mayor y auxiliares.
Dependencias  : V0002__crear_tablas_comprobante.sql
Objetos       : contabilidad.Movimiento
Motivo        : Consultas por EmpresaId, PeriodoId, CuentaId y Fecha.
Impacto       : Reduce lecturas logicas del mayor.
Destructivo   : NO
Riesgo        : BAJO
Rollback      : Roll forward; ver README.md
Ticket/ADR    : ADR-XXXX
Validacion    : validation/003_indices.sql
===============================================================================
*/
```

El runner rechaza scripts sin encabezado completo o con `Destructivo: SI` sin la aprobación registrada (§7).

# 7. Runner, versionamiento y rollback

## 7.1. Runner

Se crea `tools/Neros.Database.Deploy` (consola .NET, sin EF). Se usa uno propio y no DbUp u otra librería porque hacen falta checksum, dependencias, encabezado y validaciones. Funciones:

- `plan`: lista qué se aplicaría, sin tocar la base.
- `apply`: aplica en orden.
- `validate`: ejecuta `validation/`.
- `verify`: detecta scripts aplicados cuyo checksum cambió (falla).
- `baseline`: registra scripts existentes sin ejecutarlos.

Flujo de `apply` por script: calcular checksum → comprobar si ya se aplicó → comprobar dependencias → ejecutar → registrar resultado.

## 7.2. Tabla de versión (una por base)

`dbo.NerosSchemaVersion`: `Id`, `Modulo`, `Script`, `Tipo` (V/R/Seed), `Checksum` (SHA-256), `Descripcion`, `AplicadoEnUtc`, `AplicadoPor`, `DuracionMs`, `Exito`, `Error`.

## 7.3. Transacciones

- Por defecto, cada script se ejecuta dentro de una transacción con `SET XACT_ABORT ON` y `TRY/CATCH` con `THROW`.
- Un script con separadores `GO` se divide en lotes y el runner envuelve la transacción desde fuera, porque `TRY/CATCH` no cruza lotes.
- Operaciones no transaccionales (`CREATE INDEX ... ONLINE`, cambios de base de datos, `ALTER DATABASE`) se marcan `Transaccional: NO` en el encabezado y van solas en su script.

## 7.4. Rollback

- Se prefiere roll forward.
- Ningún `DROP COLUMN`/`DROP TABLE` automático sobre datos. Un cambio destructivo sigue expand/contract:
  1. Agregar lo nuevo.
  2. Migrar datos con un script verificable.
  3. Desplegar la aplicación compatible con ambos esquemas.
  4. Retirar lo viejo en una versión posterior, con backup y aprobación.

# 8. EF Core sin migraciones

- Permitido: `DbContext`, LINQ, `AsNoTracking`, consultas compiladas, `ExecuteUpdate/Delete`, interceptores y transacciones.
- Prohibido: `Migrations/`, `Database.Migrate()`, `EnsureCreated()`, `GenerateCreateScript()`.
- **Acciones inmediatas sobre el código actual:**
  - `tests/Neros.Tests/EntornoPruebas.cs` deja de usar `EnsureCreatedAsync()` y despliega la base temporal con el runner.
  - Se retira `--generar-sql` de `Neros.Api/Seguridad/ComandosAdministracion.cs`.
- **Prueba de deriva:** tras desplegar scripts en una base temporal, una prueba recorre el modelo EF y verifica que cada tabla, columna, tipo y nulabilidad mapeada existe igual en la base. Si el modelo y los scripts difieren, falla.

# 9. Tipos de datos

| Uso | Tipo |
|---|---|
| Importes de documento y contables | `decimal(19,4)` |
| Precio y costo unitario | `decimal(19,6)` |
| Costo promedio y valores intermedios de costeo | `decimal(28,10)` |
| Cantidades | `decimal(19,6)` |
| Porcentajes y tarifas | `decimal(9,6)` (0,190000 = 19%) |
| Tasas de cambio | `decimal(28,12)` (admite tasas inversas pequeñas) |
| Instantes | `datetime2(3)` en UTC |
| Fechas de negocio | `date` |
| Moneda | `char(3)` ISO 4217 |
| País | `char(2)` ISO 3166-1 |

Prohibidos `float`, `real`, `money` y `smallmoney` en valores financieros. Los importes persistidos se redondean según la política de redondeo (§32); los intermedios conservan la precisión mayor.

# 10. Concurrencia

- `rowversion` en toda entidad editable.
- La API expone `ETag` y exige `If-Match` al modificar. Una versión desactualizada devuelve `409 Conflict` y la UI muestra "Este registro fue modificado por otro usuario" con la opción de recargar.
- Operaciones de stock y numeración usan bloqueo pesimista acotado (§49, §52), no solo concurrencia optimista.

# 11. Escala

- Tablas de movimientos (contables, inventario) preparadas para particionado por periodo cuando el volumen lo justifique, medido y no por intuición.
- Tablas de saldos como proyección mantenida en la misma transacción que el movimiento, reconstruible (§63).
- Índices columnstore solo en tablas o proyecciones de reporte, no en tablas OLTP calientes.
- Nota de entorno: SQL Server Express limita el tamaño de base (10 GB) y los recursos; sirve para desarrollo, no como destino de producción.

# 12. Fechas y zonas horarias

- Instante (cuándo ocurrió algo técnicamente): `datetime2` UTC, por ejemplo `CreadoEnUtc`.
- Fecha de negocio (fecha contable, de emisión, de pago, de nómina): `date`, interpretada en la zona horaria IANA de la empresa. Nunca se deriva convirtiendo UTC a la hora del servidor.
- La UI muestra instantes en la zona del usuario o de la empresa, indicándolo.

# 13. Historial de datos maestros

Tablas temporales de SQL Server (`SYSTEM_VERSIONING`) para maestros cuyo historial importa: terceros, plan de cuentas, tarifas y contratos. Complementan, no sustituyen, el snapshot en documentos.

# 14. Auditoría inmutable

- `auditoria.Evento` por base: `ActorId`, `TenantId`, `EmpresaId`, `Accion`, `Modulo`, `Entidad`, `EntidadId`, `FechaUtc`, `Ip`, `UserAgent`, `CorrelationId`, `Resultado`, `Antes`/`Despues` (JSON, sin secretos y con campos sensibles enmascarados).
- El usuario de aplicación tiene solo `INSERT` y `SELECT`; `DENY UPDATE, DELETE`.
- **Tablas ledger** (disponibles en la instancia SQL Server 2025 instalada) para `auditoria.Evento` y comprobantes contabilizados. Aportan evidencia criptográfica de no alteración.
- IP y UserAgent son datos personales: su retención se rige por §27.

---

# PARTE III — IDENTIDAD, CUENTA Y SEGURIDAD

# 15. Cuenta de usuario (acordado)

- **Menú de usuario en la barra superior:** nombre, correo, empresa activa, Cambiar empresa, Cambiar contraseña, Mis sesiones, Administración (solo administrador global), idioma, tema y Cerrar sesión.
- **Cambiar mi contraseña** (`/cuenta/clave`):
  - pide la clave actual, la nueva y la confirmación, con la política vigente (mínimo 12);
  - rota el `SecurityStamp`, lo que revoca las otras sesiones, y renueva la actual;
  - registra auditoría y aplica límite de intentos.
- **Restablecer clave** (solo `AdministradorGlobal`, política en servidor):
  - genera una clave temporal aleatoria que se muestra una sola vez y nunca se guarda en claro ni en auditoría;
  - marca `DebeCambiarClave` y revoca todas las sesiones del usuario;
  - la auditoría registra el actor y el usuario afectado.
- **Cambio obligatorio:** con `DebeCambiarClave`, solo se permite `/cuenta/clave` y cerrar sesión.
- **Administración de usuarios:** listado paginado con búsqueda, estado, desbloquear y restablecer clave.

# 16. Sesiones

- **Datos de cada sesión:** `SesionId`, `UsuarioId`, `InicioUtc`, `UltimaActividadUtc`, `Ip`, `UserAgent`, `Revocada`, `RevocadaEnUtc`, `MotivoRevocacion`.
- **Acciones del usuario:** cerrar esta sesión, cerrar las demás o cerrar todas.
- **Tokens:** se guarda el hash, nunca el token en claro.

# 17. MFA

TOTP, WebAuthn/passkeys y códigos de recuperación sobre ASP.NET Identity (compatible con OpenIddict en D-06). Obligatorio progresivamente para `AdministradorGlobal`, Contabilidad, Nómina y Tesorería, según política del tenant.

# 18. Autorización

- Permisos granulares `MODULO.RECURSO.ACCION` (`CONTABILIDAD.COMPROBANTE.CONTABILIZAR`, `CONTABILIDAD.PERIODO.REABRIR`, `TESORERIA.PAGO.APROBAR`).
- Roles por ámbito (tenant, empresa, sucursal) compuestos por permisos; los roles actuales Administrador, Operador y Consulta se mapean a conjuntos iniciales.
- Decisión de autorización fresca en operaciones sensibles, sin cachés que alteren la revocación.
- **Segregación de funciones configurable** por tenant: quien crea no aprueba (orden de compra, pago, asiento). Se valida en servidor contra el actor real.

# 19. Puente de autenticación transitorio

Mientras no exista Identity/OIDC (D-06), el Gateway valida la sesión de compatibilidad contra `Neros.Api` en cada solicitud y emite un JWT interno de vida corta (≤ 60 s) por audiencia de módulo. El JWT lleva actor, tenant, empresa y permisos. Se documenta en ADR-0004 y se retira en D-06.

---

# PARTE IV — PRIVACIDAD Y DOCUMENTOS LEGALES

# 20. Enfoque

Privacidad por diseño y proporcional: primero lo que Neros trata hoy (cuentas, auditoría, terceros), después lo que traerá Nómina. El contenido jurídico lo aprueba un abogado para cada operador o cliente; el software aporta infraestructura y evidencia.

# 21. Roles jurídicos

Registrar por tratamiento: responsable (normalmente la empresa cliente), encargado (normalmente el operador de Neros), subencargados, finalidad, base o causal aplicable y contrato asociado.

# 22. Inventario de datos personales

`privacidad.CatalogoDatoPersonal`: campo, módulo, clasificación (Personal, Restringido, Sensible), finalidad, origen, retención y permiso requerido. No se agrega un campo personal sin una entrada en el catálogo (revisión de código).

# 23. Documentos legales versionados

- **Tablas:** `DocumentoLegal`, `DocumentoLegalVersion` (contenido, hash, vigencia) y `AceptacionLegal` (documento, versión, usuario, `FechaUtc`, IP, UserAgent, hash del documento aceptado, forma de aceptación).
- **Documentos:** términos y condiciones, política de privacidad, política de tratamiento de datos personales, autorización de tratamiento, política de cookies, política de seguridad, política de conservación, acuerdo de tratamiento de datos (DPA), uso aceptable y gestión de incidentes.

# 24. Derechos del titular

Flujo: solicitud → validación de identidad → clasificación → asignación → atención → respuesta → evidencia → cierre. Tipos: consulta, acceso, actualización, corrección, supresión y revocación. Plazos parametrizados por jurisdicción desde el registro normativo, no fijos en código.

# 25. Cookies y almacenamiento local

- **Inventario actual (esencial o de preferencia):**
  - cookie de autenticación y antiforgery;
  - cookie de cultura, creada solo cuando el usuario elige idioma;
  - tema en `localStorage`;
  - correo recordado en `localStorage`, solo con la casilla marcada.
- **Siempre:** página `/cookies` generada desde `privacidad.DefinicionCookie` (nombre, proveedor, categoría, finalidad, duración, primera o tercera parte, dominio, esencial, URL de política, activo).
- **Banner** (Aceptar todas, Rechazar no esenciales, Configurar) solo cuando se incorpore la primera cookie o script no esencial. Las categorías no esenciales empiezan desactivadas y sus scripts no se cargan antes del consentimiento.
- El consentimiento se versiona y se registra como una aceptación más.

# 26. Datos sensibles

Salario, cuenta bancaria e información médica (cuando exista legalmente) exigen:

- acceso mínimo con permisos separados;
- cifrado de columna o Always Encrypted evaluado caso a caso;
- enmascaramiento en la UI y prohibición de registrarlos en logs;
- exportaciones controladas y auditadas.

# 27. Retención y legal hold

- `RetencionPolicy`: tipo de información, jurisdicción, periodo, inicio del cómputo, fundamento y acción final (conservar, archivar, anonimizar, eliminar o bloquear), todo parametrizado.
- `LegalHold`: impide eliminar o anonimizar mientras haya litigio, auditoría, investigación u obligación regulatoria.
- La retención de datos contables prevalece sobre la supresión cuando la ley lo exija; la solicitud se responde indicando el bloqueo.

---

# PARTE V — GLOBALIZACIÓN Y LOCALIZACIÓN

# 28. Separación de responsabilidades

- **Idioma de UI:** preferencia del usuario (es, en, pt, ya implementado).
- **Configuración regional de la empresa:** país, moneda funcional, zona horaria, cultura de formato y marco contable. Vive en Organización (Fase 4).
- **Localización legal:** paquetes por país que aportan reglas, catálogos y formatos sin `if pais == "CO"` en el dominio.

# 29. Registro normativo

`cumplimiento.ReglaNormativa`: `Id`, `Pais`, `Jurisdiccion`, `Modulo`, `Codigo`, `Nombre`, `Descripcion`, `Fuente`, `VigenteDesde`, `VigenteHasta`, `Version`, `Estado` (POR_VERIFICAR, VERIFICADA, DEROGADA), `RevisadoEnUtc`, `RevisadoPor`.

Referencias iniciales para Colombia, **todas en estado POR VERIFICAR** hasta revisión de especialista:

- Decreto 2420 de 2015 y modificaciones (marcos contables por grupo);
- Ley 1581 de 2012 y normas reglamentarias (protección de datos);
- Resolución DIAN citada como "Resolución Única 227 de 2025" (facturación, documento soporte, nómina electrónica, RADIAN).

Documentación en `docs/compliance/<pais>/<area>/REG-<PAIS>-<AREA>-NNN.md` y fórmulas en `FORMULA-<AREA>-NNN` (§62).

# 30. Paquetes de localización

- Contrato `IPaqueteLocalizacion` y paquetes `Neros.Localizacion.Generico`, `Neros.Localizacion.Colombia`, luego México, Perú...
- **Aportan:**
  - validadores de identificación fiscal;
  - catálogos (tipos de documento, responsabilidades fiscales, municipios);
  - plan de cuentas plantilla;
  - conceptos tributarios y de nómina;
  - calendarios y formatos de documentos electrónicos.
- **Solo datos y reglas versionadas.** Un paquete no escribe en bases de otros módulos; cada módulo importa lo que le corresponde mediante seed o configuración versionada.

# 31. Multimoneda

- Por empresa: moneda funcional y, opcionalmente, moneda de presentación.
- Cada documento tiene su moneda y la tasa usada (fuente, fecha y valor).
- Cada movimiento contable guarda `ImporteMonedaDocumento`, `MonedaDocumento`, `Tasa` e `ImporteMonedaFuncional`. Los saldos y el balance se calculan en moneda funcional.
- Tabla `TasaCambio` con fuente, fecha, moneda origen y destino, y valor.

# 32. Política de redondeo

`PoliticaRedondeo` por moneda y tipo documental: `PrecisionCalculo`, `PrecisionMoneda` (decimales ISO 4217), `ModoRedondeo` (`AwayFromZero`, `ToEven`, `Truncate`) y `MomentoRedondeo` (por línea o por total). Todo redondeo pasa por un único servicio; `Math.Round` sin modo explícito queda prohibido y lo detecta un analizador.

---

# PARTE VI — DOMINIOS

# 33. Contabilidad: modelo y ecuación

- **Entidades:** Ejercicio, Periodo (incluido un periodo de ajustes de cierre opcional), CuentaContable (naturaleza explícita Débito/Crédito, tipo Activo/Pasivo/Patrimonio/Ingreso/Costo/Gasto/Orden, nivel, si admite movimiento, si exige tercero o centro de costo), CentroCosto (jerárquico), Proyecto, TipoComprobante, Comprobante, MovimientoContable y DocumentoOrigen.
- **Partida doble:** todo comprobante contabilizado cumple `Σ Débitos = Σ Créditos` en moneda funcional, tras aplicar la precisión de la moneda. Un comprobante descuadrado no se contabiliza; se valida en dominio, en la API y con un `CHECK` o validación en base.
- **Ecuación contable como invariante de prueba:**
  - antes del cierre: `Activo = Pasivo + Patrimonio + (Ingresos − Costos − Gastos)`;
  - después del cierre de resultados: `Activo = Pasivo + Patrimonio`.
  - Las cuentas de orden se excluyen.

# 34. Saldos

- **Convención única interna:** `SaldoFirmado = Σ Débitos − Σ Créditos`.
- **Presentación según naturaleza:**
  - Débito: `SaldoAnterior + Débitos − Créditos`;
  - Crédito: `SaldoAnterior + Créditos − Débitos`.
- Un saldo contrario a su naturaleza se muestra como tal y no se oculta cambiando el signo.
- **Balance de comprobación** por empresa, periodo y moneda funcional: `Σ MovimientosDébito = Σ MovimientosCrédito` y `Σ SaldosDébito = Σ SaldosCrédito`.

# 35. Cierre, reapertura y reversión

- **Estados del periodo:** Abierto → En cierre → Cerrado → (Reabierto → En cierre → Cerrado).
- **Condiciones para cerrar:** cero comprobantes descuadrados, cero documentos críticos pendientes, cero errores de integridad y cierres de módulos origen confirmados (inventario, nómina) cuando existan.
- **Periodo cerrado:** rechaza crear, modificar o anular movimientos con fecha en él, validado en servidor y en base.
- **Reapertura:** comando con permiso `CONTABILIDAD.PERIODO.REABRIR`, motivo obligatorio y auditoría. Nunca un `UPDATE` directo.
- **Corrección:** comprobante de reversión que referencia el original (`ComprobanteOriginalId`, `ComprobanteReversionId`, `Motivo`) más un nuevo comprobante correcto.
- **Cierre de resultados:** traslada ingresos, costos y gastos a resultados del ejercicio con un comprobante de cierre trazable.

# 36. Terceros

- **Maestro único de identidad:** tipo persona u organización, identificaciones por país (validadas con el paquete de localización), nombres o razón social, direcciones, contactos, responsabilidades fiscales y roles (cliente, proveedor, empleado, accionista, acreedor, deudor, otro).
- **Cuentas bancarias de proveedores:** en Terceros, con permiso restringido y auditoría.
- **Datos laborales** (contrato, salario, cuenta de nómina, afiliaciones): viven en Nómina, no en Terceros.
- **Snapshot en documentos:** identificación, razón social, dirección, ciudad, responsabilidades y correo, según lo requiera cada documento. Un cambio del tercero no modifica documentos históricos.

# 37. Motor de contabilización

`ReglaContabilizacion` y `ReglaContabilizacionDetalle` por evento de negocio (`VENTA_FACTURADA`, `COMPRA_RECIBIDA`, `NOMINA_LIQUIDADA`...):

- cada línea tiene un rol contable (`CUENTA_CLIENTE`, `CUENTA_INGRESO`, `CUENTA_IVA_GENERADO`), débito o crédito, y la fórmula del importe a partir de campos del evento;
- la cuenta concreta se resuelve por configuración (empresa, tipo de producto, impuesto, centro de costo), con vigencia;
- el comprobante generado guarda la versión de regla usada y el documento origen (`ModuloOrigen`, `TipoDocumentoOrigen`, `DocumentoOrigenId`, `NumeroDocumentoOrigen`);
- la navegación funciona en ambos sentidos: documento → comprobante → movimientos, y movimiento → comprobante → documento;
- no hay números de cuenta en C# ni en Razor.

**Ejemplo de venta** (tarifa ilustrativa; en producción, la tarifa vigente configurada):

```text
Débito  Cuentas por cobrar   1.190.000
Crédito Ingresos             1.000.000
Crédito IVA generado           190.000
Control: Débito = Crédito = 1.190.000
```

# 38. Impuestos

- **Módulo dueño de la configuración:** Impuesto, TarifaImpuesto (vigencia), ConceptoTributario, ReglaTributaria (condiciones: concepto, tipo y responsabilidades del tercero, jurisdicción, base mínima, fecha, exenciones), Retención, Autorretención y Jurisdicción.
- **Publicación:** conjuntos de reglas como **versiones inmutables**.
- **Cálculo en Ventas y Compras:** en proceso, con el motor puro `Neros.Impuestos.Motor` (sin E/S) sobre la versión vigente. Cachear una versión inmutable es seguro. Cada documento guarda la versión usada, la base, la tarifa y el resultado por impuesto.
- **Separación de bases:** base contable ≠ base fiscal; se preparan diferencias permanentes y temporarias, ajustes y conciliación fiscal sin alterar la contabilidad.

# 39. Fórmulas de impuestos

- **General:** `Impuesto = BaseGravable × Tarifa`.
- **Impuesto incluido, un solo impuesto porcentual:**
  - `Base = redondear(Total / (1 + Tarifa))`;
  - `Impuesto = Total − Base` (así Base + Impuesto = Total exactamente).
- **Varios impuestos o impuestos en cascada** (impuesto sobre base más otro impuesto): los resuelve el motor según el orden y la base de cada regla; no hay fórmula única.
- **Retenciones:** `Retención = BaseSujeta × TarifaRetención`, solo si la regla aplica. Reducen el valor a pagar o cobrar, no el total del documento.

# 40. Totales de documento

- **Por línea:**
  - `Bruto = Cantidad × PrecioUnitario`;
  - `Descuentos` según la regla;
  - `BaseNeta = Bruto − Descuentos`;
  - impuestos y retenciones de la línea.
- **Documento:**
  - `Subtotal = Σ BaseNeta`;
  - `Impuestos = Σ impuestos`;
  - `Total = Subtotal + Impuestos`;
  - `ValorAPagar = Total − Retenciones − Anticipos aplicados`.
- Total comercial, total fiscal, total contable y saldo por cobrar son conceptos distintos con campos distintos.

# 41. Estados financieros y reporting

- **Motor configurable:** `DefinicionReporte`, `SeccionReporte`, `LineaReporte`, `MapeoCuentaReporte` y `FormulaReporte`, con un lenguaje de fórmulas cerrado (`SUM`, `SUBTRACT`, `PERCENT`, `VARIATION`, `RATIO`) y sin ejecución de código de usuario.
- **Informes:** estado de situación financiera (corriente/no corriente), estado de resultados (utilidad bruta, operacional, antes de impuestos, neta), flujo de efectivo (directo e indirecto, con operación/inversión/financiación mapeables) y cambios en el patrimonio.
- **Controles:** `ActivoTotal = PasivoTotal + Patrimonio` (tras incorporar el resultado del periodo); si no se cumple, se muestra la diferencia.
- **Comparativos:**
  - `Variación = Actual − Anterior`;
  - `Variación % = (Actual − Anterior) / |Anterior| × 100`;
  - si `Anterior = 0`, se muestra "N/A".
- **Indicadores** (liquidez corriente, capital de trabajo, margen bruto, margen neto, endeudamiento): con denominador cero se muestra "N/A", nunca infinito ni error.
- **Marco contable por empresa** (`CO-GRUPO1/2/3`, configurado y validado por el contador; Neros no lo infiere). Las políticas contables (deterioro, depreciación, mediciones) se parametrizan o registran, no se deciden por software.

# 42. Inventario: movimientos y existencias

- **Movimientos inmutables:** entrada, salida, transferencia (salida más entrada vinculadas), ajuste, devolución y producción. Nunca `UPDATE Stock` sin movimiento.
- **Conceptos distintos en datos y en UI:** existencia física, reservado, disponible (`Física − Reservado`) y en tránsito.
- **Kárdex por producto y bodega:** fecha, documento, tipo, cantidad y valor de entrada, cantidad y valor de salida, saldo en cantidad y valor, y costo unitario. Reconstruible desde movimientos.

# 43. Cuentas por cobrar y por pagar

- **Modelo:** Documento CxC/CxP, Cuota (vencimiento), Aplicación (pago → documento), Nota crédito/débito y Anticipo.
- **Saldo por documento y por tercero:** `Saldo = Σ cargos − Σ abonos`.
  - En CxC los cargos son factura, nota débito e intereses; los abonos, pagos aplicados, notas crédito, retenciones practicadas por el cliente y castigos.
  - En CxP es simétrico.
- **Reconstrucción:** el saldo se reconstruye siempre desde movimientos; un campo mutable de saldo es solo una proyección.
- **Aplicaciones persistidas:** la suma aplicada de un pago más su remanente como anticipo es igual al importe del pago. Ejemplo: 1.000.000 = 300.000 + 500.000 + 200.000 de anticipo.

# 44. Tesorería

Caja, banco, cuenta bancaria, ingreso, egreso, transferencia, pago, recaudo y conciliación bancaria. Toda operación genera su evento contable y su aplicación a CxC/CxP cuando corresponde.

# 45. Diferencia en cambio

- **Realizada al pagar:** `Diferencia = Importe × (TasaPago − TasaDocumento)` en moneda funcional, sobre el importe pagado (admite pagos parciales).
  - En CxC, una diferencia positiva es ingreso y una negativa es gasto.
  - En CxP es lo contrario.
- **No realizada al cierre:** reexpresión de saldos abiertos en moneda extranjera a la tasa de cierre, con su reversión o ajuste según la política configurada.
- Las cuentas se resuelven por el motor de contabilización.

# 46. Ventas

Flujo extensible: cotización → pedido → despacho → factura → CxC → recaudo. Base de esta iteración: cotización y pedido (borrador, confirmado, anulado). El modelo prevé despacho y factura sin implementarlos todavía.

# 47. Compras

Flujo extensible: requisición → solicitud de cotización → orden de compra → recepción → factura de proveedor → CxP → pago. Base de esta iteración: orden de compra (borrador, aprobada, recibida parcial o total) y recepción, con segregación entre quien crea y quien aprueba.

# 48. Devoluciones

Siempre vinculadas al documento original cuando exista. Definen qué cantidades, impuestos, costos y asientos revierten. No hay movimientos huérfanos.

# 49. Costeo de inventario

- **Promedio ponderado (primer método):**
  - `CostoPromedioNuevo = (ValorAnterior + ValorEntrada) / (CantidadAnterior + CantidadEntrada)`, solo si el denominador es mayor que 0;
  - las salidas se valoran al costo promedio vigente en su fecha;
  - precisión interna `decimal(28,10)`.
- **Movimientos retroactivos:** una entrada o salida con fecha anterior a otros movimientos obliga a recalcular en orden cronológico los costos posteriores (job de recosteo). No se permiten movimientos con fecha dentro de un periodo de inventario cerrado.
- **Existencias negativas:** prohibidas por defecto por bodega. Si una empresa las habilita, la salida se valora al último costo conocido y se marca para ajuste al llegar la entrada.
- **Concurrencia:** las salidas bloquean la fila de existencia (producto + bodega) dentro de la transacción; una prueba verifica que dos salidas simultáneas no dejan la existencia negativa.
- **FIFO e identificación específica:** requieren capas de costo por entrada (lote, fecha, cantidad restante, costo). Se preparan en el modelo y se implementan después.
- **Costo de venta:** `Cantidad × CostoUnitarioVigente`, redondeado según la política solo al persistir.

# 50. Facturación electrónica (Colombia, posterior)

- Separar documento comercial, documento fiscal y documento electrónico.
- **Estados del electrónico:** Borrador, Generado, Firmado, Enviado, Validado, Rechazado, Entregado y Anulado según el flujo permitido. El PDF no es éxito.
- **Componentes:** factura, nota crédito, nota débito, documento equivalente electrónico, documento soporte, nómina electrónica y RADIAN. Cada uno se activa solo cuando está implementado, probado en habilitación y validado por especialista.
- **Integración:** firma y certificados en un secret store, reintentos idempotentes y consulta de estado.

# 51. Snapshot legal de documentos fiscales

Versión normativa, versión de reglas tributarias, tarifas, datos fiscales del emisor y del adquirente, resolución, rango y prefijo de numeración, moneda, tasa, impuestos, fecha de expedición (fecha de negocio más instante UTC) y versión de formato. Además, la versión de software que lo procesó.

# 52. Numeración

- **Tabla `NumeracionDocumento`:** `EmpresaId`, `TipoDocumento`, `Prefijo`, `Desde`, `Hasta`, `Actual`, `VigenciaDesde`, `VigenciaHasta`, `Resolucion`, `Estado`.
- **Asignación del número:** al emitir, no en borrador, dentro de la misma transacción que emite el documento, con `UPDATE ... SET Actual = Actual + 1 OUTPUT inserted.Actual` sobre la fila del rango (bloqueo de fila). Si la transacción falla, el número no se consume.
- **Prohibido para documentos fiscales:** `MAX + 1`, `SEQUENCE` e `IDENTITY`, porque dejan huecos.
- **Alertas:** rango por agotarse o vencer.

# 53. Nómina

- **Motores separados:** matemático, contractual, laboral, tributario y de seguridad social. Ninguna clase `CalcularNomina()` monolítica.
- **Snapshot de la liquidación:** contrato, salario, conceptos, bases, tarifas, días, horas, reglas y versión usadas. Un cambio posterior del contrato no altera una liquidación contabilizada.
- **Base de esta iteración:**
  - empleados (referencia a Terceros más datos laborales segregados);
  - contratos, conceptos de devengo y deducción definidos por el usuario, y periodos;
  - liquidación con esos conceptos.
  - La UI indica que no aplica reglas legales hasta tener el paquete de Colombia validado.

# 54. Módulos posteriores

- **Activos fijos:**
  - categoría, adquisición, vida útil, valor residual, método, depreciación, deterioro y baja;
  - lineal: `(Costo − ValorResidual) / VidaÚtil` por periodo, según política.
- **Presupuesto:**
  - versión, cuenta, centro de costo, periodo y valor;
  - `Variación = Real − Presupuesto`;
  - `Ejecución % = Real / Presupuesto × 100`, con "N/A" si el presupuesto es 0.
- **Producción:** lista de materiales, ruta, orden, consumos, devoluciones, terminados, merma y costos (materia prima, mano de obra, indirectos).
- **Proyectos:** `ProyectoId` opcional en movimientos de ingresos, gastos, compras, horas y presupuesto.

---

# PARTE VII — INTEGRACIÓN Y PROCESOS

# 55. Outbox e Inbox

- **Outbox** por base (`integracion.MensajeSalida`): `Id`, `EventoId` (único), `TipoEvento`, `VersionEvento`, `TenantId`, `EmpresaId`, `AgregadoId`, `OcurridoEnUtc`, `CorrelationId`, `CausationId`, `Payload`, `ProcesadoEnUtc`, `Intentos`, `UltimoError`. Se escribe en la misma transacción que el cambio de negocio.
- **Inbox** por consumidor (`integracion.MensajeEntrada`): `EventoId` único para descartar duplicados. El procesamiento y la marca de recibido van en la misma transacción.
- **Ruta crítica:** sin broker (D-03 BLOQUEADA), los eventos quedan en Outbox sin despachar. Los flujos entre módulos (pedido → reserva, recepción → existencias, factura/compra/nómina → contabilidad) dependen de habilitar D-03.

# 56. Idempotencia

`Idempotency-Key` obligatorio en pagos, emisión de documentos fiscales, importaciones e integraciones. Se guarda la clave con el hash de la solicitud y la respuesta; una repetición devuelve la misma respuesta.

# 57. Jobs e importaciones

- **Estados de un job:** Pendiente, En ejecución, Exitoso, Fallido y Cancelado, con progreso, reintentos, lease y cola de mensajes fallidos.
- **Importaciones:** archivo → prevalidación → vista previa → errores → confirmación → procesamiento → reporte.
- **Archivo de errores:** Excel con fila, campo, valor, código de error y descripción.
- **Nunca** "subir Excel e insertar directo".

# 58. APIs

- `/api/v1/<modulo>/...` con ProblemDetails, CorrelationId, paginación (`page`, `pageSize`, `sort`, `filter`, `search`), validación, autorización, ETag/If-Match e Idempotency-Key cuando aplique.
- Listados siempre paginados en servidor con tamaño máximo.

---

# PARTE VIII — EXPERIENCIA Y DESIGN SYSTEM

# 59. Dashboard (acordado)

- Solo datos reales, sin métricas inventadas.
- **Contenido:**
  - cabecera con saludo, empresa activa y fecha en la zona de la empresa;
  - tarjetas de módulos con estado real, que muestran indicadores reales al habilitarse cada módulo (pedidos en borrador, productos sin existencia, periodo contable abierto);
  - tarjeta de cuenta y seguridad (último acceso, aviso de clave temporal, sesiones abiertas);
  - actividad reciente compacta;
  - panel de administración solo para el administrador global.
- **Orden en móvil:** módulos, cuenta, actividad.

# 60. Componentes y estados

- **`NerosDataGrid`:**
  - paginación, orden y filtros en servidor;
  - búsqueda, filtros guardados, columnas configurables, densidad, acciones rápidas, selección y navegación por teclado;
  - exportación sujeta a permisos y enmascaramiento, y como job cuando es grande.
- **Estados en cada página:** cargando, vacío, sin resultados, error, prohibido, no disponible y éxito. Nunca un spinner infinito.
- **Responsive, i18n (es/en/pt) y tema claro/oscuro obligatorios**, siguiendo `DESIGN.md`.

# 61. UX contable

- **Captura rápida por teclado:** tabulación, autocompletado de cuenta, tercero y centro de costo, duplicar, insertar o eliminar línea, y validación inmediata.
- **Totales fijos visibles:** débito, crédito y diferencia. "Contabilizar" permanece deshabilitado mientras la diferencia sea distinta de 0, y el servidor valida por su cuenta.

---

# PARTE IX — CALIDAD, RENDIMIENTO Y OPERACIÓN

# 62. Documentación de fórmulas y reglas

Cada fórmula tiene su ficha `FORMULA-<AREA>-NNN` (nombre, fórmula, módulo, entradas, precisión, redondeo, casos límite y pruebas `...-T01`) y cada regla normativa su `REG-<PAIS>-<AREA>-NNN`, enlazada al registro normativo.

# 63. Pruebas

- **Unitarias de dominio:** con valores límite (0, 0,01, negativos permitidos, miles de líneas, decimales, redondeos, moneda extranjera, periodos, concurrencia, reversión y anulación).
- **Invariantes:**
  - débito = crédito;
  - la ecuación contable de §33;
  - cantidad en inventario = movimientos acumulados;
  - saldo CxC/CxP = cargos − abonos.
- **Reconstrucción:** eliminar en prueba las proyecciones (existencias, saldos contables, saldos de tercero) y reconstruirlas desde movimientos con resultado idéntico.
- **De integración:** sobre base temporal desplegada con el runner, más la prueba de deriva EF (§8).
- **De API:** códigos, ProblemDetails, 401/403/409, tenant cruzado y revocación.
- **De navegador:** para flujos principales, con capturas en ambos temas, dos tamaños y tres idiomas.

# 64. CI/CD de SQL

1. Crear una base temporal.
2. `apply` de todas las carpetas.
3. `validate`.
4. `verify` (checksums).
5. Ejecutar pruebas de integración.
6. Destruir la base temporal.

Cualquier fallo detiene el pipeline.

**Análisis estático progresivo:**

- `SELECT *`, `NOLOCK` y tipos financieros prohibidos;
- tablas sin PK y FK sin índice;
- scripts sin encabezado, duplicados o destructivos sin aprobación;
- `Math.Round` sin modo explícito.

# 65. Rendimiento

- **Objetivos orientativos:** P50 menor que 200 ms y P95 menor que 800 ms en operaciones interactivas normales. No son garantía contractual sin pruebas y dimensionamiento.
- **Optimización con evidencia:** plan de ejecución, lecturas lógicas, CPU, duración, cardinalidad y sensibilidad de parámetros.
- **Índices** por patrones reales de consulta.

# 66. Observabilidad y logs

- **Cada solicitud registra:** TraceId, CorrelationId, TenantId, EmpresaId, UsuarioId, módulo, endpoint, duración y estado (OpenTelemetry, ya adoptado).
- **No se registran payloads completos.**
- **Se enmascaran:** contraseñas, tokens, cookies, números bancarios, salarios y documentos sensibles.

# 67. Continuidad

- **Backups:** full, diferencial y log, con retención, cifrado, copia fuera del sitio y pruebas de restauración periódicas.
- **RPO y RTO definidos** por ambiente y criticidad.

---

# PARTE X — ROADMAP RECONCILIADO

Cada fase termina con build, pruebas, capturas y documentación antes de la siguiente. Las fases marcadas con ⚠ saltan gates de D-XX con la autorización del usuario del 2026-09-25 (ADR-0004).

| Fase | Contenido | Relación con D-XX | Criterio de salida |
|---|---|---|---|
| 0 | ADR-0004 (salto de gates, BD por módulo con scripts, puente JWT, Outbox sin despacho), convenciones SQL, este plan aprobado | D-01 (extiende) | ADR aceptado |
| 1 | Runner `Neros.Database.Deploy`, `NerosSchemaVersion`, baseline de `compatibilidad`, pruebas sin `EnsureCreated`, retiro de `--generar-sql`, prueba de deriva EF, CI SQL | D-04 (parcial) ⚠ | Suite verde desplegando solo con scripts |
| 2 | Cuenta y seguridad (§15–§18): menú, cambio y restablecimiento de clave, sesiones, auditoría con actor, permisos granulares iniciales | D-06 (anticipa) ⚠ | Pruebas API y navegador; 403 y revocación |
| 3 | Privacidad mínima: catálogo de datos, documentos legales versionados con aceptación, `/cookies`, retención de auditoría | Transversal | Aceptación registrada; `/cookies` generada |
| 4 | Tenant/Organización: Tenant → Grupo → Empresa → Sucursal, configuración regional de la empresa, correspondencia empresa → tenant, puente JWT | D-05, D-07 ⚠ | Solicitud de punta a punta a un módulo con tenant y permisos |
| 5 | Design System: dashboard (§59), navegación por módulos, `NerosDataGrid`, estados | — | Capturas en 2 tamaños, 2 temas y 3 idiomas |
| 6 | Globalización: catálogos ISO, `Dinero`, redondeo, tasas, fechas de negocio, registro normativo, paquete genérico | — | Pruebas de redondeo y conversión |
| 7 | Terceros | D-08 ⚠ | CRUD, búsqueda, identificación validada, historial temporal |
| 8 | Impuestos: configuración versionada y motor puro | D-13 (parte) ⚠ | Casos de §39 y §40 probados |
| 9 | Contabilidad base: plan, periodos, comprobantes, reversión, cierre, balance de comprobación, motor de contabilización | D-13 ⚠ | Invariantes de §33 a §35 |
| 10 | Inventario con promedio ponderado | D-09 ⚠ | Concurrencia, recosteo y reconstrucción del kárdex |
| 11 | Ventas: cotización y pedido | D-10 ⚠ | Totales de §40 y snapshot |
| 12 | Compras: orden y recepción | D-15 (parte) ⚠ | Segregación de funciones y recepciones parciales |
| 13 | **Broker real (D-03)** y flujos entre módulos: pedido → reserva, recepción → existencias, eventos → contabilización | D-03, D-11 | Ensayos de duplicados, desorden, caída y reproceso |
| 14 | CxC y CxP | D-14 ⚠ | Saldos por aplicaciones y reconstrucción |
| 15 | Tesorería | D-13/D-14 | Conciliación bancaria |
| 16 | Contabilidad avanzada: estados financieros, reporting configurable y diferencia en cambio | D-13 | Controles de §41 |
| 17 | Facturación electrónica Colombia | D-16 | Habilitación DIAN y validación de especialista |
| 18 | Nómina base | D-18b ⚠ | Snapshot, segregación y permisos |
| 19 | Nómina legal y electrónica Colombia | D-18b | Validación de especialista |
| 20+ | Activos, presupuesto, producción, integraciones, BI/analítica | D-16 a D-18 | Según su plan |

**Nota sobre el broker:** conviene adelantarlo (Fase 13) en cuanto haya un RabbitMQ accesible, por ejemplo con Docker o el instalador local. Mientras siga bloqueado, los módulos de las Fases 7 a 12 funcionan aislados.

---

# PARTE XI — DEFINITION OF DONE

## Técnica

| Área | Obligatorio |
|---|---|
| Dominio | Reglas e invariantes con pruebas unitarias |
| API | Endpoint versionado, ProblemDetails, paginación, ETag cuando aplique |
| SQL | Script `V` con encabezado, constraints, índices justificados y validación |
| SQL | Prueba de deriva EF en verde |
| Seguridad | Permisos granulares, tenant y empresa, segregación cuando aplique |
| Auditoría | Eventos append-only sin secretos |
| UX | Cargando, vacío, error, prohibido; responsive; teclado |
| i18n | Textos en es, en y pt |
| Calidad | Pruebas unitarias, de integración, de API y SQL |
| Rendimiento | Medición de la operación principal |
| Documentación | README del módulo, CHANGELOG SQL y fichas de fórmula |
| Arquitectura | ADR si cambia una decisión |

## Legal (funcionalidades reguladas)

Norma identificada, vigencia y fuente registradas, regla documentada, casos límite, pruebas, validación de especialista, fecha de revisión y responsable.

---

# PARTE XII — CUMPLIMIENTO Y REGLA FINAL

# 68. Advertencia de cumplimiento

Neros implementa reglas contables, tributarias, laborales y de protección de datos, pero no se autodeclara jurídicamente conforme por tenerlas. Antes de habilitar una función regulada en producción se requieren:

- revisión técnica y funcional;
- revisión contable y tributaria;
- revisión jurídica cuando corresponda;
- pruebas con casos reales controlados;
- validación de la normativa vigente.

El sistema facilita el cumplimiento y conserva evidencia; no sustituye el juicio profesional.

# 69. Regla final

El propósito no es poder crear una factura, sino poder responder años después, sin leer código:

- quién la creó, la modificó y la aprobó;
- qué configuración, impuesto, tarifa y norma vigente usó;
- qué versión del software la procesó;
- qué asiento generó, qué inventario movió y qué saldo originó;
- qué datos fiscales tenía el tercero en ese momento;
- qué documento electrónico se transmitió, qué respondió la autoridad y quién recibió el resultado;
- qué cambió después.

Si Neros responde eso, es un ERP. Si solo crea, edita, elimina y lista, es un CRUD grande.

---

# PARTE XIII — ESTADO DE IMPLEMENTACIÓN (2026-09-26)

Este apartado no sustituye el roadmap D-01..D-18 ni la validación legal; resume el avance técnico respecto a las **fases 0–26** de la Parte X.

| Ámbito | Estado | Notas |
|---|---|---|
| Fases 0–4 | **Hecho** | Scripts SQL, cuenta/seguridad, privacidad, tenant/organización y puente JWT |
| Fase 5 | **Hecho** | Design System, dashboard, `NerosDataGrid`, capturas i18n |
| Fase 6 | **Hecho** | Dominio + SQL `globalizacion` (V0001–V0002), API `GET api/globalizacion/catalogos` |
| Fases 7–26 | **Base hecha** | SQL + dominio (+ servicios donde aplica); muchos módulos aún sin UI/API operativa completa |
| Administración SaaS | **Hecho** | Plataforma (empresas, logos, usuarios) y empresa (miembros por rol/permiso) |
| D-03 integración | **Hecho** | Outbox/inbox, worker, indexación búsqueda, comprobante automático |
| Gates D-05–D-18 | **Parcial / productivo** | Extracción definitiva, OIDC (D-06), UI por módulo y producción regulada pendientes de despliegue |

**Catálogos transversales (todas las empresas):** viven en el módulo `database/globalizacion/` (sin `EmpresaId`): países, monedas, zonas horarias, unidades de medida, tipos de identificación por país, tasas, redondeo y registro normativo. Despliegue en `NEROSGLOBAL` (o co-aplicado en entorno de pruebas). Configurar `ConnectionStrings:Globalizacion` en la API de compatibilidad.

**Próximo trabajo recomendado (post-plan):** UI operativa por módulo de negocio, habilitación productiva DIAN/nómina con especialista, extracción Organization/Identity (D-05/D-06) y despliegue de bases por módulo en el entorno del usuario.
