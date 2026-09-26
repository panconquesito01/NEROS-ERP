# Modulo `contabilidad`

Incluye reporting configurable (V0003, §41) y marco contable por empresa.

Plan de cuentas, ejercicios y periodos, comprobantes y movimientos con partida doble. Invariantes en `Neros.Domain/Contabilidad`.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo contabilidad --servidor localhost --base NEROSCONT --crear-base
```
