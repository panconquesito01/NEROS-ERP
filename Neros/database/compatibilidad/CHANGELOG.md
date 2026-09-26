# Changelog `compatibilidad`

## V0002 — 2026-09-25

- `AspNetUsers.DebeCambiarClave` (bit, default 0) para el cambio obligatorio tras un restablecimiento.
- `Sesiones`: `Id` publico unico (`UX_Sesiones_Id`), `InicioUtc`, `UltimaActividadUtc`, `Ip`, `AgenteUsuario`, `RevocadaEnUtc` y `MotivoRevocacion`. Las sesiones existentes quedan activas con inicio igual a la fecha de aplicacion.
- Esquema `auditoria` y tabla `auditoria.Evento` como ledger append-only (SQL Server 2022 o superior).
- Solo agrega columnas con valor por defecto y objetos nuevos; no es destructivo.

## V0001 — 2026-09-25

- Adopción del esquema inicial bajo el runner (ADR-0004). No hay cambios en tablas ni índices respecto al script anterior; solo se agregan el encabezado y el esquema `dbo` explícito, y se quita la transacción manual.
