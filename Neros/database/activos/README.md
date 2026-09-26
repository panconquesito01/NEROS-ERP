# Modulo `activos`

Activos fijos (plan fase 20, §54): categoria, adquisicion, depreciacion lineal, deterioro y baja.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo activos --servidor localhost --base NEROSACTIVOS --crear-base
```
