# Módulo `compatibilidad`

Esquema del host de compatibilidad `Neros.Api`: ASP.NET Identity, empresas, membresías, sesiones opacas y eventos de acceso. Base de datos: `NEROSERP` (esquema `dbo`, nombres heredados de Identity).

| Script | Contenido |
|---|---|
| `V0001__identity_empresas.sql` | Esquema inicial (antes `database/scripts/001_identity_empresas.sql`) |
| `V0002__cuenta_sesiones_auditoria.sql` | Clave temporal, datos de sesión y revocación, `auditoria.Evento` (ledger append-only) |

- Validaciones: `validation/001_esquema_compatibilidad.sql` y `validation/002_cuenta_sesiones_auditoria.sql`.
- Bases creadas con el script antiguo: adoptarlas con `baseline --hasta V0001` y después `apply`, que aplica `V0002`; ver [DEPLOYMENT_GUIDE](../conventions/DEPLOYMENT_GUIDE.md).
- `auditoria.Evento` requiere SQL Server 2022 o superior (ledger).
- El modelo EF de `Neros.Persistence` debe coincidir con este esquema; la prueba de deriva lo comprueba.
