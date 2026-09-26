# Modulo `terceros`

Maestro de identidad de terceros (persona u organizacion), identificaciones por pais, roles base y cuentas bancarias de proveedor. Historial temporal en `terceros.Tercero` (`SYSTEM_VERSIONING`).

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo terceros --servidor localhost --base NEROSTERCEROS --crear-base
dotnet run --project ../../tools/Neros.Database.Deploy -- validate --modulo terceros --servidor localhost --base NEROSTERCEROS
```

Configurar `ConnectionStrings:Terceros` en `Neros.Terceros.Api`.
