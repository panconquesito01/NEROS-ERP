# Guía de despliegue de base de datos

Herramienta: `tools/Neros.Database.Deploy`. Convenciones: [SQL_CONVENTIONS](SQL_CONVENTIONS.md).

## Comandos

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- <comando> --modulo <modulo> --servidor <servidor> --base <base> [opciones]
```

| Comando | Qué hace | Modifica la base |
|---|---|---|
| `plan` | Muestra los scripts pendientes (`V`), los reaplicables (`R`, `S`) con cambios y los problemas de verificación | No |
| `apply` | Verifica y aplica en orden: `V` pendientes, `R` cambiados, `S` cambiados; después ejecuta `validate` | Sí |
| `validate` | Ejecuta `validation/`; falla si alguna consulta devuelve filas | No |
| `verify` | Falla si un script aplicado cambió de checksum o desapareció | No |
| `baseline --hasta V<NNNN>` | Registra como aplicados los `V` hasta el indicado sin ejecutarlos, en bases que ya tienen ese esquema | Solo `NerosSchemaVersion` |

| Opción | Uso |
|---|---|
| `--raiz <ruta>` | Carpeta `database/`; por defecto se busca subiendo desde el directorio actual |
| `--crear-base` | Crea la base si no existe (solo `apply` y `baseline`) |
| `--conexion-variable <VAR>` | Toma la cadena de conexión de una variable de entorno en lugar de `--servidor`/`--base` con seguridad integrada |
| `--permitir-destructivos` | Necesaria para aplicar scripts con `Destructivo: SI` y `Aprobacion` |
| `--timeout <segundos>` | Tiempo máximo por lote; por defecto 300 |

Código de salida: 0 correcto, 1 error de despliegue o validación, 2 uso incorrecto.

## Registro de versiones

Cada base tiene `dbo.NerosSchemaVersion`. Guarda por script: módulo, nombre, tipo (`V`/`R`/`S`), checksum SHA-256 (con finales de línea normalizados a LF), descripción, fecha UTC, usuario SQL (`SUSER_SNAME()`), duración, éxito y error. Los intentos fallidos también quedan registrados.

## Flujo de un script

1. Validar nombre, encabezado y análisis estático.
2. Comprobar que no está aplicado y que su checksum no cambió.
3. Comprobar que sus dependencias están aplicadas.
4. Ejecutar sus lotes dentro de una transacción (si es `Transaccional: SI`).
5. Registrar el resultado en la misma transacción. Si falla, revertir, registrar el fallo y detener el despliegue.

## Adoptar una base existente (baseline)

La base local `NEROSERP` ya tiene el esquema de compatibilidad. Para adoptarla sin reejecutar nada:

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- plan --modulo compatibilidad --servidor localhost --base NEROSERP
dotnet run --project .\tools\Neros.Database.Deploy -- baseline --modulo compatibilidad --servidor localhost --base NEROSERP --hasta V0001
dotnet run --project .\tools\Neros.Database.Deploy -- validate --modulo compatibilidad --servidor localhost --base NEROSERP
```

`baseline` solo se permite si el módulo no tiene registros previos en la base. Ejecutar `validate` después para confirmar que el esquema real coincide.

## Base nueva

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo compatibilidad --servidor localhost --base NEROSERP --crear-base
```

## Pruebas y validación continua

La suite `tests/Neros.Tests` hace de pipeline SQL: crea bases temporales, aplica todos los módulos con el runner, ejecuta `validate` y `verify`, y compara el modelo EF con la base (prueba de deriva). Si un script falla, la suite falla. No hay workflow de GitHub Actions hasta que se apruebe explícitamente.

## Cambios destructivos

Seguir expand/contract:

1. Agregar lo nuevo.
2. Migrar los datos con un script verificable.
3. Desplegar la aplicación compatible con ambos esquemas.
4. Retirar lo viejo en un script posterior con `Destructivo: SI`, `Aprobacion`, backup previo y `--permitir-destructivos`.

Se prefiere roll forward: no hay rollback automático de datos.
