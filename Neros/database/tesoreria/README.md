# Modulo `tesoreria`

Caja, banco, movimientos, transferencias y conciliacion bancaria (§44). Logica en `Neros.Domain/Tesoreria`.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo tesoreria --servidor localhost --base NEROSTESORERIA --crear-base
```
