# Base de datos Neros ERP

## Instalacion

Requiere SQL Server en localhost y permisos de Windows para crear el esquema. La cuenta de ejecucion de la API debe tener permisos de lectura y escritura, no permisos de administrador del servidor.

1. Ejecutar `000_crear_base.sql`; no modifica una base existente.
2. Ejecutar `001_identity_empresas.sql` una sola vez sobre una base vacia. Crea Identity, empresas, membresias, sesiones y auditoria dentro de una transaccion.
3. Crear el administrador con el comando local indicado en la documentacion tecnica. No insertar claves ni hashes manuales en SQL.

```powershell
sqlcmd -S localhost -E -C -b -i .\database\scripts\000_crear_base.sql
sqlcmd -S localhost -E -C -b -d NEROSERP -i .\database\scripts\001_identity_empresas.sql
dotnet run --project .\Neros.Api --launch-profile http -- --inicializar-admin
```

El esquema inicial se genera desde el mapeo de EF Core, se revisa y se ejecuta manualmente; no existen migraciones EF ni aplicacion automatica del esquema al iniciar la API. No reejecutar el script 001 sobre tablas existentes. Los cambios posteriores requieren nuevos scripts numerados.

## Seguridad y operacion

- La cadena local reside en User Secrets de la API bajo `ConnectionStrings:Neros`, no en estos archivos.
- `-C` acepta el certificado local; en produccion usar un certificado confiable y no omitir su validacion.
- El rol pertenece a `UsuariosEmpresas`, nunca al usuario global.
- Desactivar una membresia impide inmediatamente las siguientes consultas a esa empresa.
- Desactivar un usuario o cambiar su sello de seguridad invalida sus sesiones.
- No borrar auditoria para corregir errores operativos.
- Sesiones vencidas: ejecutar periodicamente `DELETE FROM dbo.Sesiones WHERE Expira <= SYSDATETIMEOFFSET();` mediante un job administrado.
- Definir politica de retencion y archivo para auditoria antes de produccion.

## Reversion

El script 001 usa `XACT_ABORT` y transaccion. Ejecutar con `sqlcmd -b` para detenerse ante el primer error. Si ya hay datos, tomar un backup y preparar una reversion especifica; no borrar tablas automaticamente. El script 000 no tiene reversion destructiva.

Ver [documentacion tecnica](../docs/tecnica/acceso-multiempresa.md).