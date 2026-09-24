# NEROS: estado de ejecucion

Fecha: 2026-09-13. Fuente: [ADR-0002](../adr/ADR-0002-distributed-modular-platform.md) y [roadmap](../NEROS_ERP_IMPLEMENTATION_ROADMAP.md). La autorizacion de implementacion posterior sustituye la restriccion de entrega solo documental; no cambia arquitectura ni autoriza produccion/commit/push.

## Current initiative

D-01: COMPLETED. D-02: COMPLETED. D-03: BLOCKED por falta de broker accesible para el piloto real. D-04 a D-18: NOT_STARTED. No saltar gates.

## Completed gates

I-01 revalidada: referencia Microsoft.AspNetCore.OpenApi ausente y ambos paquetes OpenAPI ausentes en assets de API/Tests. No es una nueva auditoria de todas las dependencias.

D-01: PASS. Correspondencia PRUEBA -> NEROS-TEST autorizada expresamente, validate contra SQL exit 0 sin cambios de datos. Suite 37/37 PASS, cuatro pruebas de mapping incluyen manifiesto estable, multiples tenants, empresa futura sin correspondencia y rechazo de CompanyId como TenantId. Build de once proyectos PASS (5.9 s). Contratos, threat model, proveedores, revocacion/fail closed, administrador global, recuperacion y estrategia de corte registrados. No se certifica persistencia de tenant ni subsistemas futuros por este gate.

D-02: PASS. [Gateway/BFF y Organization](D02_GATEWAY_ORGANIZATION.md) implementados sin SQL nuevo. Suite completa 41/41 PASS, cero omitidas; diez regresiones de acceso pasan ahora por Gateway con SQL temporal y navegador. Cuatro pruebas Gateway/Organization cubren contexto firmado, headers descartados, health, traza entre hosts, upstream detenido, Identity indisponible y concurrencia 32/33. Build trece proyectos PASS, NuGet sin vulnerabilidades conocidas en fuente oficial. No habilita broker, base Organization ni proveedor OIDC productivo.

## Pending gates

D-03: broker piloto real, adaptador IEventBus, Outbox/Inbox SQL locales, dispatcher y ensayos de crash/confirm/ACK/lease/duplicados/desorden/DLQ/replay. Los puertos definidos en D-01 no son implementacion durable. D-04 requiere D-03; no adelantar migracion D-05 ni declarar aislamiento SQL tenant implementado.

## Current blockers

Bloqueo contractual resuelto por autorizacion expresa, no por inferencia del nombre. [Manifiesto estable](../../deploy/development/company-tenant-map.json): NEROS-TEST, TenantId f70c8608-2f4a-4d79-90b8-30f1b569bd17 -> PRUEBA, CompanyId 42336010-6818-443b-9166-d03ab1754478 preservado. Sin grupo inventado, fallback o asignacion automatica futura. Snapshot local anterior bajo obj es evidencia historica, no la fuente aprobada. Nuevas empresas sin mapping detienen validacion. Tenant dedicado a pruebas no equivale a infraestructura dedicada ya desplegada.

Comprobacion de entorno al entrar a D-03: sin docker/podman/rabbitmqctl/rabbitmq-server/erl en PATH, sin servicios Docker/RabbitMQ, sin Docker en ruta habitual, cero listeners AMQP 5671/5672 y cero variables Rabbit/Broker/AMQP. No hay broker configurado/accesible para el gate. Se necesita habilitar Docker/RabbitMQ local o proporcionar acceso a un broker de pruebas mediante configuracion segura. No instalar con elevacion interactiva ni poner credenciales en chat/repositorio. No se han solicitado ni utilizado recursos cloud/productivos. No usar simulacion en memoria como sustituto del gate de persistencia.

## Important decisions

Plataforma distribuida SaaS, bases y propietarios independientes. Tenant -> BusinessGroup -> Company -> Branch, multiples empresas por tenant, mapping explicito, mismos contratos para shared/sharded/dedicated. Usuario confirma AdministradorGlobal con acceso a todo: concesion de plataforma vigente, tenant seleccionado y auditoria, no eliminar filtros tenant ni confiar en headers. Revocacion vigente/fail closed aprobados, sin ventana de permisos obsoletos. API, BFF, Organization y Gateway consumen foundation; Gateway:BaseUrl es opt-in compatible para no romper el arranque anterior. Mensajeria hoy son contratos, no almacenamiento durable. Organization tecnico valida JWT/contexto, no sustituye membresias vigentes de D-05.

## Migrations

Ninguna. NEROSERP y cuentas existentes no modificadas. Herramienta tools/Neros.Organization.Migration con snapshot/inspect/validate: SELECT unicamente, snapshots locales CreateNew sin sobrescribir. Manifiesto aprobado versionable en deploy/development/company-tenant-map.json: datos de desarrollo explicitos, no registros ya migrados. Cuatro pruebas cubren IDs estables, multiples empresas/tenants, grupos cruzados, duplicados e inventario cambiado. D-05 debe persistir TenantId antes de generar datos nuevos de negocio; permisos/jobs/eventos/Outbox/Inbox/archivos/Search/Analytics lo conservan en sus respectivos gates.

## Tests

I-01: PASS estructural. Suite completa final: 41/41 PASS, cero omitidas (diez regresiones SQL/navegador por Gateway, veintitres foundation, cuatro mapping y cuatro Gateway/Organization). HTTP Kestrel: codigos 400/401/403/404/409/412/429/503, excepcion JSON y Accept HTML sin detalle privado en respuesta/log capturado, correlacion, GET retries/POST unico/timeout, JWT issuer/audience/firma/exp, scope/permiso/tenant, readiness y spans de hosts reales. Hash: escala/orden equivalentes; rechaza precision perdida/underflow/overflow. NuGet: trece proyectos sin vulnerabilidades conocidas en fuente oficial consultada. No se ha probado broker/collector remoto, carga ERP ni redaccion universal de todos los adaptadores.

Comandos ejecutados: dotnet test tests/Neros.Tests/Neros.Tests.csproj --no-restore --tl:off -v minimal; dotnet list Neros.slnx package --vulnerable --include-transitive --no-restore --source https://api.nuget.org/v3/index.json; dotnet build Neros.slnx --no-restore -v minimal. Build final PASS: trece proyectos, 5.8 s. Graphify update PASS parcial: grafo de codigo 1418 nodos/1769 aristas; no parser SQL, siete archivos sin nodos y semantica documental no regenerada. No confundir grafo con validacion SQL.

Diagnosticos de documentos/codigo nuevos sin errores reportados. El editor conserva alerta obsoleta Microsoft.OpenApi 2.0.0 en csproj de tests; ambos paquetes OpenAPI ausentes en assets actuales API/Tests, restore/build/auditoria oficial correctos. Informes locales excluidos por git check-ignore. Verificacion final: cero procesos Neros y cero listeners en puertos de desarrollo conocidos. Sin commit/push ni cambios a datos/cuentas existentes.

## Next action

Habilitar broker de pruebas accesible y ejecutar D-03 real; no saltar a D-04/D-05. Mantener TenantId obligatorio y autorizacion por ambito al implementar permisos/jobs/eventos/Outbox/Inbox/archivos/Search/Analytics, sin afirmar que el manifiesto implementa esos subsistemas. NEROS-TEST es un dato de desarrollo, no politica de asignacion. Ningun servidor Neros queda abierto al finalizar.