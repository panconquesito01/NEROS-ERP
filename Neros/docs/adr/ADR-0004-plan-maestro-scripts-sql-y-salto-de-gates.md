# ADR-0004: plan maestro, esquema por scripts SQL y salto autorizado de gates

Fecha: 2026-09-25. Estado: **ACCEPTED** por aprobacion explicita del responsable del producto del [plan maestro 2.1](../execution/PLAN_MAESTRO_NEROS_ERP.md). Complementa [ADR-0002](ADR-0002-distributed-modular-platform.md) y [ADR-0003](ADR-0003-foundation-development.md); no los sustituye.

## Contexto

El roadmap D-01..D-18 exige gates secuenciales. D-03 (broker) sigue BLOCKED por falta de RabbitMQ accesible y D-04..D-18 estaban NOT_STARTED. El responsable del producto pidio avanzar cuenta y seguridad, dashboard, globalizacion y los primeros modulos (Terceros, Impuestos, Contabilidad, Inventario, Ventas, Compras, Nomina) sin esperar esos gates, con reglas estrictas de base de datos, trazabilidad financiera y cumplimiento.

El repositorio crea hoy el esquema con un script generado desde EF (`GenerateCreateScript`) y las pruebas usan `EnsureCreated()`, lo que contradice la politica de scripts como fuente de verdad.

## Decisiones

1. **Plan maestro 2.1 como plan de ejecucion vigente.** Sus fases 0..20 sustituyen el orden de D-04..D-18. Cada fase del plan indica que D-XX anticipa. Las fases marcadas con ⚠ saltan gates con esta autorizacion; los controles de cada gate (pruebas, seguridad, tenant) siguen siendo obligatorios dentro de la fase.
2. **Esquema administrado solo por scripts SQL versionados.** Prohibidos EF Migrations, `Database.Migrate()`, `EnsureCreated()` y `GenerateCreateScript()` para crear o cambiar esquema, incluido en pruebas. EF Core se mantiene para acceso a datos.
3. **Runner propio `tools/Neros.Database.Deploy`.** Aplica `database/<modulo>/migrations` (secuenciales e inmutables), `repeatable` y `seed`; ejecuta `validation`; registra cada script con checksum SHA-256 en `dbo.NerosSchemaVersion` de cada base; rechaza scripts aplicados modificados, encabezados incompletos, dependencias ausentes y cambios destructivos no aprobados. Se elige runner propio y no DbUp u otra libreria porque se requieren encabezado obligatorio, dependencias, analisis estatico y validaciones con la misma herramienta.
4. **Una base por modulo con esquemas SQL por modulo**, sin FK, vistas ni joins entre bases, conforme a ADR-0002. La base de compatibilidad `NEROSERP` se adopta como modulo `compatibilidad` mediante baseline de su script existente.
5. **Puente de autenticacion transitorio.** Hasta Identity/OIDC (D-06), el Gateway valida la sesion de compatibilidad en cada solicitud, sin cache, y emite JWT internos de vida corta por audiencia de modulo. Se retira en D-06.
6. **Outbox sin despacho mientras D-03 este bloqueado.** Los modulos persisten eventos en su Outbox local en la misma transaccion; ningun flujo entre modulos se declara implementado hasta tener broker real (fase 13 del plan).
7. **Validacion SQL dentro de la suite de pruebas.** Despliegue completo por scripts sobre base temporal, validaciones, verificacion de checksums y prueba de deriva entre el modelo EF y la base. No se agrega workflow de GitHub Actions: `.github/workflows/README.md` exige aprobacion explicita para automatizacion de build.

## Consecuencias

- Mas trabajo por cambio de esquema: script con encabezado, validacion y prueba de deriva. A cambio, el esquema es revisable, reproducible y auditable.
- Los modulos construidos antes del broker operan aislados; la consistencia entre modulos llega con la fase 13.
- Identidad y empresas siguen en compatibilidad hasta las fases 2 y 4; los cambios alli crean deuda de migracion documentada.
- El salto de gates no autoriza produccion, datos reales de clientes, commit ni push.

## Verificacion

Fase 1 cierra cuando la suite completa pasa desplegando la base de pruebas solo con el runner, sin `EnsureCreated`, con prueba de deriva EF en verde y `--generar-sql` retirado. La evidencia se registra en el [estado de ejecucion](../execution/NEROS_EXECUTION_STATUS.md).
