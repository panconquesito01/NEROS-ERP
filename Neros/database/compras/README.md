# Modulo `compras`

Orden de compra (borrador, aprobada, recibida parcial o total) y recepciones con segregacion creador/aprobador (§47). Totales §40 e snapshot de proveedor al aprobar. Logica en `Neros.Domain/Compras`.

## Despliegue

```powershell
dotnet run --project ../../tools/Neros.Database.Deploy -- apply --modulo compras --servidor localhost --base NEROSCOMPRAS --crear-base
```
