# D-01: contratos, amenazas y transicion

Fecha: 2026-09-13. Implementacion: [ServiceDefaults](../../src/BuildingBlocks/Neros.ServiceDefaults/FoundationHttp.cs), [contextos](../../src/BuildingBlocks/Neros.ServiceDefaults/SecurityContext.cs), [autenticacion](../../src/BuildingBlocks/Neros.ServiceDefaults/ServiceAuthentication.cs), [mensajeria](../../src/BuildingBlocks/Neros.Messaging.Abstractions/IntegrationEnvelope.cs). Decisiones en [ADR-0003](../adr/ADR-0003-foundation-development.md).

## Contratos de borde

| Contrato | Semantica / consumidor |
| --- | --- |
| TenantContext | TenantId obligatorio; CompanyId y BranchId solo del ambito validado. Sucursal requiere empresa. Sin tenant por defecto. Organization D-02/D-05 |
| SecurityContext | Solo principal autenticado por JWT validado, un sub/tenant/exp no ambiguos; permisos exactos, sin equivalencia rol global. Allows comprueba ambito del recurso y expiracion. No sustituye decision fresca de membresia de D-05 |
| Error HTTP | ProblemDetails con code estable, correlationId y traceId; excepcion inesperada sin mensaje interno. 400/401/403/404/409/412/429/503 diferenciados; BFF conserva su UX/estados actuales |
| Correlacion | X-Correlation-Id Guid no vacio, sustituir entrada invalida; propagar solo normalizado. Traceparent W3C para trazas, nunca autorizacion |
| Health | /health/live minimo anonimo; /health/ready exige Platform.Health.Read y no publica detalles de dependencias. Hoy no certifica readiness de SQL/broker, sus checks llegan con el adaptador |
| Organization v1 | D-02: GET /api/v1/organization/context devuelve subject/tenant/ambito ya autenticados, sin inventar empresa persistida; requiere organization.read + Organization.Context.Read. D-05 agrega recursos reales y validacion actualizada de membresia |
| API actual | api/acceso y api/empresas preservadas durante transicion, autentican sesiones existentes. No crear claims tenant falsos a partir de EmpresaId |
| Eventos Organization v1 | CompanyChanged.v1 y MembershipChanged.v1: ID/version de origen, tenant, ambito y estado minimo; CompanyDisabled.v1/PermissionChanged.v1 para invalidacion. No publicar eventos antes de implementar su escritura real en D-05 |

## Mensajeria e idempotencia

IntegrationEnvelope inmutable: MessageId, Type.version, Producer, TenantId, CompanyId, AggregateId/Version, OccurredAt UTC, ActorId opcional, CorrelationId, CausationId, TraceParent y objeto Data clonado de hasta 64 KiB. Payload especifico vive con propietario, no en una biblioteca ERP universal. No tokens, passwords o documentos completos por conveniencia. Campos aditivos compatibles; cambios de significado requieren nueva version y dedupe funcional comun durante convivencia.

IEventBus.PublishAsync confirma aceptacion durable/routing del broker, no efecto remoto. IOutbox.AddAsync solo agrega al contexto/transaccion local del caso, NO ejecuta commit separado. IInbox.TryRegisterAsync usa Consumer+MessageId unico y participa en la MISMA transaccion que efecto/auditoria/Outbox resultante. No construir implementaciones en memoria y registrarlas como durables. En D-03 el dispatcher reclama lease, publica fuera de transaccion SQL, confirma y marca; un crash entre publish/marca puede duplicar.

RequestFingerprint canonicaliza propiedades ordenadas, arrays en orden y numeros decimales sin escala redundante; hash SHA-256. Aplicar a DTO validado/normalizado por el propietario; no usar para contrasenas. No incluye autorizacion. Clave persistida por tenant/empresa/operacion/idempotencyKey, hash y resultado se confirman con efecto. Misma clave/hash reproduce; hash diferente 409; maximo/retencion definidos por caso. No es persistencia de idempotencia ni algoritmo RFC 8785 universal; numeros fuera de decimal rechazados. Deben versionarse DTO/normalizacion antes de cambiar sus reglas.

## Threat model y evidencia

| Amenaza | Control actual / evidencia | Pendiente antes del uso real |
| --- | --- | --- |
| Header tenant/actor falsificado | Resolver ignora headers; HTTP con tokens firmados en FoundationAuthenticationTests | Organization decision fresca y aislamiento SQL D-05 |
| JWT firma/emisor/audiencia/vigencia invalidos | JwtBearer estricto, RS256/ES256, skew cero; pruebas negativas HTTP | Proveedor real, rotacion/revocacion, service credentials D-06 |
| Rol plataforma usado como permiso empresarial | Permiso explicito y recurso/ambito; FoundationSecurityTests | SoD y administracion de permisos D-05/D-07 |
| Error expone secreto | ProblemDetails sanitizado, header conservado aun con excepcion | Politica de redaccion collector/logs operativos y revision de cada adaptador |
| Pago repetido por retry | POST/PUT/PATCH/DELETE sin retry automatico; GET 3 intentos, POST 1 y timeout real probados | Idempotencia SQL del propietario antes de escrituras nuevas |
| Perdida/doble mensaje | Envelope validado; contrato Outbox/Inbox local explicito | D-03 SQL/broker real, ACK/crash/DLQ/replay; hoy no hay garantia durable implementada |
| Readiness expone infraestructura | Permiso operativo, respuesta estado minimo; usuario normal 403 | Probes de SQL/broker por servicio, no DB ajena |
| Confusion de traza e identidad | Correlacion UUID y W3C no otorgan autorizacion; spans de dos hosts probados | Budgets/retencion/sampling y collector D-04 |
| Recursos consumidos por cliente | Limite/concurrencia/timeout de HTTP saliente y limites actuales de login | Quotas distribuidas/tenant y proteccion de proxy D-02/D-07 |

## Migracion y autorizacion

No DDL en D-01. En D-05: Empresas/UsuariosEmpresas y eventos de seleccion -> Organization; usuarios/credenciales/sesiones y eventos login -> Identity. Preservar IDs externos sin FK entre bases. Snapshot/backfill por herramienta temporal con permisos acotados; comparar conteos/claves y pertenencias, pausa de escritura, delta/validacion, revocar escritor origen, habilitar destino. Reverso despues de corte requiere reconciliar delta, no activar dos escritores. Scripts manuales por servicio con checksum/lock y prueba sobre copia; nunca EnsureDeleted en NEROSERP.

Decision del usuario: multiples tenants, cada uno con multiples empresas; Tenant -> BusinessGroup -> Company -> Branch. Una empresa tiene un solo tenant contractual. Ningun DefaultTenant ni tenant nullable en tablas nuevas de negocio. Grupo opcional en la correspondencia inicial si no existe agrupacion empresarial, nunca perteneciente a otro tenant. Claves unicas, jobs, eventos/Outbox/Inbox, archivos, cache, Search y Analytics conservan el tenant. Shared/sharded/dedicated comparten modelo/contratos y se resuelven por routing, no por forks ni SQL cruzado.

AdministradorGlobal tiene acceso a todo por decision explicita del usuario. Migrar la concesion a permiso de plataforma validado, no a una membresia limitada inventada ni a un rol empresarial universal. El servicio de autorizacion permite seleccionar cualquier tenant a esa identidad, verifica concesion vigente, emite ambito explicito y audita actor/tenant/accion. Los servicios empresariales mantienen filtros y contexto de un tenant; el rol textual solo no pasa FoundationSecurityTests. Una sesion normal multi-tenant sigue resolviendo permisos por membresia independiente. La revocacion/fail closed fue aprobada por el usuario, sin ventana de permisos obsoletos.

La herramienta [Neros.Organization.Migration](../../tools/Neros.Organization.Migration/Program.cs) prepara inventario SQL solo lectura y rechaza correspondencias incompletas/duplicadas/desconocidas o grupos cruzados. Version 1 del documento de preparacion permite TenantId nulo SOLO como dato pendiente, nunca como dato migrado aceptable. Los tenants aprobados tienen ID y codigo explicitos; un CompanyId no puede reutilizarse como TenantId.

Autorizacion expresa del usuario: PRUEBA pertenece al tenant dedicado a desarrollo/pruebas NEROS-TEST. [Manifiesto autorizado](../../deploy/development/company-tenant-map.json): TenantId f70c8608-2f4a-4d79-90b8-30f1b569bd17, CompanyId preservado 42336010-6818-443b-9166-d03ab1754478. Sin grupo empresarial inventado. Estos identificadores son datos de migracion estables, no constantes de runtime ni valores por defecto. Ninguna empresa futura hereda este tenant; inventario diferente o correspondencia ausente es conflicto y detiene la validacion. El snapshot pendiente anterior bajo obj conserva evidencia historica, pero el manifiesto versionable es la fuente autorizada y no se pierde con dotnet clean.

El tenant es exclusivo para estos datos de pruebas; no implica infraestructura fisica dedicada ya aprovisionada. No hay DDL ni corte de propietario en D-01. D-05 debe persistir TenantId obligatorio y aislamiento antes de generar datos empresariales nuevos. Permisos, jobs, eventos/Outbox/Inbox, archivos, caches, Search y Analytics deben conservarlo y autorizar por ese ambito en sus respectivos gates. El manifiesto no certifica subsistemas todavia inexistentes.

```powershell
dotnet run --project ./tools/Neros.Organization.Migration -- snapshot neros-api-desarrollo ./Neros.Persistence/obj/migration/company-tenant-map.local.json
dotnet run --project ./tools/Neros.Organization.Migration -- validate neros-api-desarrollo ./deploy/development/company-tenant-map.json
```

Snapshot usa CreateNew: nunca sobreescribe un mapping trabajado. Archivo local fuera de versionado; contiene solo IDs, sin nombres personales/credenciales. Conexion de User Secrets o variables de entorno, nunca argumentos con credenciales. Validacion compara contra inventario vivo en cada ejecucion; no prueba congelacion del corte ni cuenta con comandos DDL/DML. D-05 debe repetirlo bajo la pausa de escritura y verificar restricciones/backup/restore. Pruebas sinteticas cubren dos tenants, varias empresas, grupos, inventario cambiado y duplicados.

Plan de recuperacion: backup probado, cuenta existente preservada, CLI Development restringida al propietario; no reabrir un endpoint anonimo para recuperar administrador. No cambiar hashes ni invalidar sesiones silenciosamente; incompatibilidad de proveedor exige reautenticacion planificada, no reset masivo.

## Gate D-01

Gate tecnico de contratos: HTTP/JWT/seguridad/trazas/resiliencia y serializacion ejecutados; validar nuevamente build/suite tras cada cambio. Gate completo del roadmap incluye revision del mapping inicial, roles, recuperacion y ADRs. La autorizacion NEROS-TEST resuelve la correspondencia contractual pendiente; debe pasar validate contra SQL y regresiones antes de registrar COMPLETED. La migracion efectiva y pruebas de corte siguen siendo D-05; mensajeria durable sigue siendo D-03.

Consumidores: API/BFF ya usan HTTP/telemetria/resiliencia; Organization/Gateway usaran autenticacion/contexto en D-02; D-03 implementa puertos de mensajeria. No extender estos componentes sin un consumidor concreto. Estado y comandos de validacion en [registro de ejecucion](NEROS_EXECUTION_STATUS.md).