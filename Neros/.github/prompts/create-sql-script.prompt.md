---
description: 'Crear script SQL Server versionado para Neros con encabezado, validacion y despliegue por runner.'
mode: agent
---

# Crear Script SQL - Neros

Crear scripts versionados en `database/<modulo>/migrations/` siguiendo [SQL_CONVENTIONS](../../database/conventions/SQL_CONVENTIONS.md) y [DEPLOYMENT_GUIDE](../../database/conventions/DEPLOYMENT_GUIDE.md) (ADR-0004).

## Reglas

- SQL Server. Nombre `V<NNNN>__<descripcion_en_snake_case>.sql`, siguiente numero libre del modulo.
- Encabezado obligatorio completo; el runner rechaza encabezados incompletos.
- Un script aplicado nunca se modifica: una correccion es un script nuevo.
- Sin `BEGIN TRANSACTION`/`COMMIT` en scripts transaccionales: el runner los envuelve.
- Sin EF Migrations, `EnsureCreated()` ni `GenerateCreateScript()`.
- Sin datos sensibles, passwords, tokens ni connection strings.
- Cambios destructivos solo con expand/contract, `Destructivo: SI` y `Aprobacion` registrada.
- Agregar o actualizar la validacion en `database/<modulo>/validation/`.

## Validacion

```powershell
dotnet run --project .\tools\Neros.Database.Deploy -- plan --modulo <modulo> --servidor localhost --base <BaseTemporal> --crear-base
dotnet run --project .\tools\Neros.Database.Deploy -- apply --modulo <modulo> --servidor localhost --base <BaseTemporal>
dotnet run --project .\tools\Neros.Database.Deploy -- validate --modulo <modulo> --servidor localhost --base <BaseTemporal>
```

No ejecutar contra una base real sin autorizacion explicita.
