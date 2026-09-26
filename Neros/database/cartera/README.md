# Modulo `cartera`

CxC y CxP con movimientos inmutables, cuotas, pagos/recaudos y aplicaciones (§43). Logica en `Neros.Domain/Cartera`.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo cartera --servidor localhost --base NEROSCARTERA --crear-base
```
