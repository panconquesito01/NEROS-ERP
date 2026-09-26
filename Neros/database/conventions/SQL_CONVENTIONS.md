# Convenciones SQL de Neros

Fuente: [plan maestro 2.1](../../docs/execution/PLAN_MAESTRO_NEROS_ERP.md) Parte II y [ADR-0004](../../docs/adr/ADR-0004-plan-maestro-scripts-sql-y-salto-de-gates.md). El runner `tools/Neros.Database.Deploy` hace cumplir las reglas marcadas con **(runner)**.

## Estructura por modulo

```text
database/<modulo>/
├── migrations/   V0001__crear_esquema.sql, V0002__crear_tabla_cuenta.sql ...   secuenciales e inmutables
├── repeatable/   R__vista_saldos.sql                                             se reaplican si cambia su checksum
├── seed/         S0001__catalogo_monedas.sql                                     datos de referencia idempotentes
├── validation/   001_tablas.sql                                                  deben devolver 0 filas
├── README.md
└── CHANGELOG.md
```

- Nombres **(runner)**:
  - `V<NNNN>__<descripcion>.sql`, `R__<descripcion>.sql`, `S<NNNN>__<descripcion>.sql`;
  - descripción en minúsculas `snake_case`;
  - números de `V` sin huecos ni duplicados.
- Un `V` aplicado no se modifica **(runner: `verify` y `apply` fallan si cambia su checksum)**. Una corrección es un `V` nuevo.
- `repeatable/` solo contiene objetos recreables sin pérdida de datos: vistas, funciones, procedimientos y permisos. Usar `CREATE OR ALTER`.
- `seed/` solo contiene catálogos de referencia, con `MERGE` o `IF NOT EXISTS`. Nunca datos de clientes ni credenciales.
- `validation/` contiene consultas que devuelven una fila por cada problema encontrado, con una columna `Problema` legible.

## Encabezado obligatorio (runner)

```sql
/*
===============================================================================
Neros ERP
Script        : V0002__crear_tabla_cuenta.sql
Modulo        : contabilidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Crea el plan de cuentas por empresa.
Dependencias  : V0001__crear_esquema.sql
Objetos       : contabilidad.CuentaContable
Motivo        : Base del motor contable (fase 9).
Impacto       : Tabla nueva, sin datos existentes afectados.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward; ver README.md
Ticket/ADR    : ADR-0004
Validacion    : validation/001_tablas.sql
===============================================================================
*/
```

| Campo | Regla |
|---|---|
| `Script` | Igual al nombre del archivo |
| `Modulo` | Igual a la carpeta del módulo |
| `Fecha` | `yyyy-MM-dd` |
| `Autor`, `Descripcion`, `Objetos`, `Motivo`, `Rollback`, `Validacion` | Obligatorios, no vacíos |
| `Dependencias` | `Ninguna` o nombres de scripts del mismo módulo separados por coma; deben estar aplicados antes |
| `Destructivo` | `SI` o `NO`. `SI` exige `Aprobacion` y el flag `--permitir-destructivos` |
| `Transaccional` | `SI` (por defecto) o `NO` para operaciones que no admiten transacción |
| `Riesgo` | `BAJO`, `MEDIO` o `ALTO` |
| `Impacto`, `Ticket/ADR`, `Aprobacion` | Opcionales salvo lo indicado |

Los valores pueden continuar en líneas siguientes con sangría.

## Transacciones

- El runner envuelve cada script `Transaccional: SI` en una transacción con `XACT_ABORT ON`, incluso si tiene varios lotes separados por `GO`.
- **(runner)** Un script transaccional no puede contener `BEGIN TRAN`, `COMMIT` ni `ROLLBACK`.
- `GO` debe ir solo en su línea; `GO n` no se admite.
- `Transaccional: NO` es para `CREATE INDEX ... WITH (ONLINE = ON)`, `ALTER DATABASE` y similares. Esos scripts van solos, y si fallan el runner no puede revertirlos: documentar cómo reanudarlos.

## Análisis estático (runner)

Se analiza el SQL sin comentarios ni literales de texto.

| Regla | Motivo |
|---|---|
| `DROP TABLE`, `DROP COLUMN`, `TRUNCATE TABLE`, `ALTER TABLE ... DROP` solo con `Destructivo: SI` | Pérdida de datos |
| Prohibido `float`, `real`, `money` y `smallmoney` | Precisión financiera (§9 del plan) |
| Prohibido `NOLOCK` y `READUNCOMMITTED` | Lecturas sucias |
| Prohibido `SELECT *` | Contratos de columnas explícitos |

## Nombres de objetos

- **Esquema SQL por módulo** (`contabilidad.Comprobante`), sin prefijos de tabla. La base de compatibilidad conserva `dbo` y los nombres de ASP.NET Identity.
- Tablas y columnas en PascalCase, en español y en singular (salvo las heredadas de compatibilidad).

| Objeto | Convención |
|---|---|
| PK | `PK_<Tabla>` |
| FK | `FK_<Tabla>_<TablaDestino>_<Campo>` |
| Índice | `IX_<Tabla>_<Campos>` |
| Único | `UX_<Tabla>_<Campos>` |
| Check | `CK_<Tabla>_<Regla>` |
| Default | `DF_<Tabla>_<Campo>` |

- FK solo entre tablas de la misma base. Entre módulos se guarda el ID externo más un snapshot.
- Toda FK con índice de soporte, salvo justificación en el encabezado.

## Columnas estándar de tablas de negocio

`Id`, `TenantId`, `EmpresaId`, `CreadoPor`, `CreadoEnUtc`, `ModificadoPor`, `ModificadoEnUtc` y `Version rowversion`. Los índices de consulta empiezan por `TenantId, EmpresaId` cuando el filtro los incluye.

## Tipos de datos

| Uso | Tipo |
|---|---|
| Importes de documento y contables | `decimal(19,4)` |
| Precio y costo unitario | `decimal(19,6)` |
| Costo promedio e intermedios de costeo | `decimal(28,10)` |
| Cantidades | `decimal(19,6)` |
| Porcentajes y tarifas | `decimal(9,6)` (0,190000 = 19 %) |
| Tasas de cambio | `decimal(28,12)` |
| Instantes | `datetime2(3)` UTC, sufijo `Utc` |
| Fechas de negocio | `date`, en la zona horaria de la empresa |
| Moneda | `char(3)` ISO 4217 |
| País | `char(2)` ISO 3166-1 |
| Textos | `nvarchar(n)` con longitud explícita; `nvarchar(max)` solo justificado |

## Índices y consultas

- Crear índices a partir de consultas reales, justificando el `Motivo` y el `Impacto` en el encabezado.
- SQL siempre parametrizado desde la aplicación.
- Listados paginados en servidor.
