# Integracion (Outbox / Inbox)

Las tablas `integracion.MensajeSalida` y `integracion.MensajeEntrada` viven **en cada base de modulo** transaccional (ventas, inventario, compras, contabilidad), aplicadas con `V0002__integracion_outbox_inbox.sql` de cada modulo.

## Broker local (fase 13)

```powershell
docker compose -f deploy/development/docker-compose.rabbitmq.yml up -d
dotnet run --project src/Workers/Neros.Integration.Worker
```

Prueba de publicacion real (opcional): `NEROS_REQUIRE_BROKER_TESTS=1 dotnet test --filter IntegracionBrokerTests`.

## Consumidores persistentes (fase 24)

El worker `Neros.Integration.Worker` suscribe colas duraderas por modulo (`neros.inventario.reservas`, `neros.inventario.entradas`, `neros.contabilidad.posting`) y procesa con inbox SQL + `ProcesadorInboxIntegracion` (duplicados, desorden, recepcion dual inventario/contabilidad).

Connection strings opcionales en el worker: `Inventario`, `Contabilidad` (ademas de `Ventas` para outbox).

Pruebas E2E sin broker: `dotnet test --filter IntegracionE2E`.

Validacion SQL: `database/integracion/validation/002_consumidores_e2e.sql` en cada base de modulo.
