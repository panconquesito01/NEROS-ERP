# Modulo organizacion

Base propia del servicio `Neros.Organization.Api` (convencion `NEROSORG` en local). Modelo contractual **Tenant → Grupo empresarial → Empresa → Sucursal** y configuracion regional por empresa.

| Carpeta | Contenido |
|---|---|
| `migrations/` | Esquema versionado |
| `seed/` | Tenant NEROS-TEST |
| `validation/` | Comprobaciones post-`apply` |

Las filas de `organizacion.Empresa` usan el mismo `Id` que `dbo.Empresas` en compatibilidad; no hay FK entre bases. Sincronizar correspondencia al dar de alta empresas nuevas.

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo organizacion --servidor localhost --base NEROSORG --crear-base
```
