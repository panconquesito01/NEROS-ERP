# Módulo `privacidad`

Catálogo de datos personales, documentos legales versionados, aceptaciones, definición de cookies y políticas de retención. Comparte la base `NEROSERP` con `compatibilidad` (esquema `privacidad`).

| Script | Contenido |
|---|---|
| `V0001__esquema_privacidad.sql` | Esquema y tablas del inventario, legal, cookies y retención |
| `S0001__catalogo_datos_compatibilidad.sql` | Inventario inicial (cuentas, sesiones, auditoría) |
| `S0002__definiciones_cookies.sql` | Cookies y almacenamiento local usados hoy |
| `S0003__documentos_legales_base.sql` | Términos y privacidad v1 (texto operativo; revisión jurídica pendiente) |
| `S0004__politicas_retencion.sql` | Retención de auditoría, sesiones y aceptaciones |

Despliegue (después de `compatibilidad`):

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo privacidad --servidor localhost --base NEROSERP
```
