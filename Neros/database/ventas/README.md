# Modulo `ventas`

Cotizacion y pedido (borrador, confirmado, anulado) con totales comerciales (§40) y snapshot de cliente e impuestos al confirmar (§36, §46). Logica en `Neros.Domain/Ventas`.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo ventas --servidor localhost --base NEROSVENTAS --crear-base
```
