# Modulo `nomina`

Nomina base (fase 18) y legal/electronica Colombia (fase 19, §50, §53): paquete `CO-LEGAL` versionado, aportes parametrizados, nomina electronica con CUNE. La semilla S0001 queda en `PendienteEspecialista` hasta aprobacion registrada.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo nomina --servidor localhost --base NEROSNOMINA --crear-base
```
