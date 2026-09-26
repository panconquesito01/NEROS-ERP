# Modulo `inventario`

Movimientos inmutables, existencia por producto/bodega, costeo por promedio ponderado (§49). Logica en `Neros.Domain/Inventario`.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo inventario --servidor localhost --base NEROSINV --crear-base
```
