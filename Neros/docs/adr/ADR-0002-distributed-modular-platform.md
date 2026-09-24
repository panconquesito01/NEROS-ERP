# ADR-0002: Distributed Modular ERP Platform

Fecha: 2026-09-13. Estado: **ACCEPTED** como decision arquitectonica por solicitud explicita del responsable del producto. Sustituye [ADR-0001](0001-monolito-modular.md). Los servicios, contratos y despliegues descritos aun no estan implementados. I-02 del roadmap anterior queda detenida; esta entrega solo modifica documentacion.

## Contexto y evidencia

Neros tiene ocho proyectos, Blazor/BFF, una API y un NerosDbContext que agrupa Identity, sesiones, empresas, membresias y eventos. RepositorioEmpresas consulta usuarios y claims con relaciones SQL. El grafo y el contexto fueron revisados directamente para esta decision. No existen dominios transaccionales que obliguen a migrar ventas o stock historicos. La evidencia y las diez pruebas de la auditoria anterior permanecen en el [diagnostico](../NEROS_ERP_ARCHITECTURE_ASSESSMENT.md); no son una prueba de autonomia distribuida.

El requisito estrategico es SaaS multi-tenant, evolucion y despliegue independientes, aislamiento y escalado selectivo de dominios durante la vida del producto. La distribucion es una decision de destino desde ahora, no una reaccion futura a la saturacion de un monolito.

## Decision

Adoptar DDD con bounded contexts de negocio suficientemente grandes, servicios autonomos y arquitectura orientada a eventos. Cada servicio posee exclusivamente reglas, datos, DbContext(s) internos, base(s), scripts de evolucion, contratos y eventos. Ningun DbContext, cuenta SQL o transaccion abarca dos servicios. Bases logicamente independientes pueden compartir instancia SQL inicialmente; no hay FK, vistas, linked servers, joins ni escrituras entre bases de servicios. IDs externos y contratos sustituyen las FK externas.

La direccion inicial de limites es Identity, Organization/IAM, Master Data, Sales, Purchasing, Inventory & Costing y Finance; Workflow, Jobs, Integration y Notification son capacidades autonomas de plataforma. Production, HCM/Payroll y Projects se crean al existir su primer flujo real. Analytics y Search mantienen proyecciones separadas. Gateway y BFF son bordes, no propietarios de reglas ERP. No crear todos estos proyectos vacios.

Finance posee GL, CxC, CxP, Treasury y posting; otros servicios nunca escriben su ledger. Inventory posee producto operativo, bodegas, stock, reservas, movimientos, lotes, seriales y costeo. Master Data posee identidad de terceros y catalogos de referencia, no todas las configuraciones empresariales. Organization posee tenant/grupo/empresa/sucursal y permisos empresariales; Identity autentica y gestiona identidad, no decide permisos del ERP.

Dentro del contexto se usan transacciones locales e invariantes fuertes. Entre servicios se usan APIs versionadas y eventos, consistencia eventual, process managers/sagas persistidas y compensaciones cuando corresponda. No usar transacciones distribuidas como solucion general ni esconder un fallo remoto dentro de un exito financiero.

Disenar IEventBus como puerto tecnico de Application para entrega desde Outbox, sin broker en Domain. Cada productor confirma cambios y Outbox en su transaccion local; cada consumidor confirma Inbox/dedupe, efectos locales y Outbox resultante juntos. Entrega at-least-once, consumidores idempotentes y recuperacion verificable; sin promesa de exactly-once. RabbitMQ es la opcion inicial propuesta para desarrollo y piloto portable; Azure Service Bus es alternativa gestionada si se elige Azure. La eleccion de produccion requiere prueba operativa. Kafka se reserva para un caso de streaming/replay medido.

Browser -> Neros Web/BFF -> Gateway -> Services. OIDC con proveedor probado, identidades de servicio, scopes, audiencia, TLS y autorizacion en cada receptor; red interna no confiable. TenantContext y SecurityContext validados, nunca headers arbitrarios. OTel y propagacion W3C, errores sanitizados, health, contenedores y CI/CD por servicio son foundation, no mejoras tardias.

## Consecuencias y costos aceptados

- Autonomia de despliegue, ownership y escalado; posibilidad de mover un tenant o servicio sin mover todo Neros.
- Mayor costo operativo: broker, bases, certificados, rotacion, trazas, pipelines, reconciliacion y soporte de fallos parciales desde el primer vertical.
- Consistencia eventual visible: un pedido confirmado puede esperar reserva; una factura emitida puede esperar contabilizacion. UI y contratos distinguen aceptacion, ejecucion y finalizacion.
- Contratos backward-compatible y pruebas productor/consumidor necesarias. Ningun cambio obliga a desplegar todos los servicios simultaneamente.
- No usar bibliotecas compartidas de entidades, reglas, EF o repositorios. Shared kernel minimo y versionable; un cambio suyo no exige actualizar a todos de inmediato.
- Ownership del proceso no equivale a ownership de datos: Jobs programa y controla progreso; workers de cada dominio ejecutan sus reglas con acceso solo a su base.

## Alternativas

| Alternativa | Resolucion |
| --- | --- |
| Monolito modular como destino | Sustituido por requisito explicito de autonomia desde el diseno |
| Servicio por tabla o entidad | Rechazado: fragmenta invariantes y genera llamadas y releases acoplados |
| Servicios con base/DbContext compartidos | Rechazado: no hay independencia real ni aislamiento de datos |
| Reescritura de login y cuentas | Rechazada: adaptar y migrar por propietario con regresiones |
| Kafka o Kubernetes obligatorios desde hoy | Rechazado: broker y orquestador responden a necesidades demostradas |
| Transaccion distribuida para venta/inventario/finanzas | Rechazada como patron general; saga, idempotencia y reconciliacion |

## Transicion y criterios de aceptacion

Conservar la solucion actual como origen temporal, no destino. Extraer Identity y Organization preservando IDs, cuentas y revocacion; un solo escritor por dato en cada paso, corte controlado y sin dual-write no atomico. Los nuevos dominios nacen con bases, hosts y pipelines propios. Las rutas BFF existentes permanecen compatibles mediante un adaptador temporal con fecha de retiro.

Primer vertical: tercero -> habilitacion comercial de cliente -> producto -> cotizacion -> pedido -> reserva de inventario. Debe demostrar Outbox/Inbox ante reinicio, duplicacion y desorden, tenant isolation, trazas, errores parciales y despliegue independiente Sales v2 con Inventory v1. Factura y Finance se agregan despues de aprobar este gate, no antes de probar la integracion por eventos.

La [arquitectura objetivo](../NEROS_ERP_TARGET_ARCHITECTURE.md) define matrices, contratos y garantias. El [roadmap revisado](../NEROS_ERP_IMPLEMENTATION_ROADMAP.md) define orden exacto, migraciones, costos relativos y gates. No se autorizan implementaciones funcionales en esta entrega ni commit/push.

## Decisiones acotadas pendientes

Proveedor OIDC y compatibilidad de hashes/sesiones; proveedor cloud y broker productivo; SLA de revocacion y convergencia; politica de retencion y residencia; RPO/RTO y perfil de carga; umbrales de aprobacion y localizacion fiscal. Estas decisiones no reabren el destino distribuido ni SaaS multi-tenant. Registrar ADR especifico antes de fijar la tecnologia o migrar datos correspondiente.