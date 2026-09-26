# Modulo `produccion`

Produccion (plan fase 22, §54): lista de materiales, ruta, orden, consumos, devoluciones, terminados, merma y costos.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo produccion --servidor localhost --base NEROSPRODUCCION --crear-base
```
