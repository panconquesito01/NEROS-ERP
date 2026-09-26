# Modulo `facturacion`

Facturacion electronica Colombia (plan fase 17, §50–§52, §56): documento comercial, fiscal y electronico separados; numeracion fiscal sin huecos; snapshot legal; idempotencia y reintentos de transmision.

La habilitacion DIAN en produccion y la validacion por especialista quedan fuera del alcance automatico de esta iteracion; el conector expone ambiente de habilitacion (sandbox) y estados del flujo electronico.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo facturacion --servidor localhost --base NEROSFACTURACION --crear-base
```
