# NEROS: estado de ejecucion

Fecha: 2026-09-26. Fuente: [ADR-0002](../adr/ADR-0002-distributed-modular-platform.md), [roadmap](../NEROS_ERP_IMPLEMENTATION_ROADMAP.md), [plan maestro 2.1](PLAN_MAESTRO_NEROS_ERP.md) y [ADR-0004](../adr/ADR-0004-plan-maestro-scripts-sql-y-salto-de-gates.md). La autorizacion de implementacion posterior sustituye la restriccion de entrega solo documental; no autoriza produccion/commit/push.

## Current initiative

Plan maestro **2.2** (2026-09-26): **implementacion base de fases 0–26 COMPLETED** — ver Parte XIII en [PLAN_MAESTRO_NEROS_ERP.md](PLAN_MAESTRO_NEROS_ERP.md). Siguiente: UI/API operativa por modulo de negocio, despliegue de bases en entornos reales y gates productivos D-05–D-18 (OIDC, extraccion definitiva, validacion legal).

D-01: COMPLETED. D-02: COMPLETED. D-03: COMPLETED. D-04: PARCIAL (runner + scripts; CI formal opcional). D-05 a D-18: BASE TECNICA en roadmap/plan; produccion y extraccion definitiva NOT_STARTED salvo puente actual.

## Administracion SaaS (D-07 / plan fase 2+4)

PASS (2026-09-26).
- SQL `compatibilidad` V0003: logo de empresa.
- API `api/admin/plataforma` (empresas, logo, usuarios con membresia) y `api/admin/empresa` (miembros; cabecera `X-Neros-Company-Id`).
- Permisos de empresa en sesion Bearer al enviar cabecera de empresa; gateway enruta plataforma/empresa/globalizacion.
- BFF: `/administracion/plataforma/empresas`, `/administracion/usuarios` (alta global), `/administracion/empresa/usuarios`; logo en selector, sidebar y admin plataforma via `/empresas/{id}/logo`.
- Pruebas: `AdministracionPlataformaTests`, `AdministracionEmpresaTests`.

Despliegue: `apply --modulo compatibilidad` (V0003). Configurar `ConnectionStrings:Globalizacion` si los catalogos no comparten base con compatibilidad.

## Globalizacion: catalogos transversales V0002

PASS (2026-09-26).
- SQL `V0002__catalogos_transversales.sql` + seed `S0006`: `ZonaHoraria`, `UnidadMedida`, `TipoIdentificacion` (sin tenant/empresa).
- API `GET api/globalizacion/catalogos?pais=CO` (autenticada).
- Pruebas: `GlobalizacionSqlTests` (ampliada), `GlobalizacionApiTests`.
- Entorno de pruebas: co-despliegue `globalizacion` en la misma base temporal que compatibilidad.

## Plan maestro: fases 0 y 1

Fase 0: PASS.
- ADR-0004 ACCEPTED.
- AGENTS, CONVENTIONS, CODEX, instrucciones y skills EF/SQL alineados con "esquema solo por scripts".
- Nuevas [SQL_CONVENTIONS](../../database/conventions/SQL_CONVENTIONS.md) y [DEPLOYMENT_GUIDE](../../database/conventions/DEPLOYMENT_GUIDE.md).

Fase 1: PASS.
- Runner `tools/Neros.Database.Deploy`:
  - comandos `plan`, `apply`, `validate`, `verify` y `baseline`;
  - `dbo.NerosSchemaVersion` con checksum SHA-256;
  - bloqueo `sp_getapplock` por modulo;
  - transaccion por script, con el fallo registrado;
  - encabezado obligatorio;
  - analisis estatico: destructivos no declarados, float/money, NOLOCK, `SELECT *` y control de transaccion.
- `database/scripts/001` pasa a `database/compatibilidad/migrations/V0001__identity_empresas.sql` sin cambio de tablas ni indices, con su validacion; `000_crear_base.sql` se sustituye por `--crear-base`.
- Pruebas sin `EnsureCreated`: `EntornoPruebas` despliega con el runner.
- `--generar-sql` retirado de `Neros.Api`.
- Prueba de deriva EF (columnas, tipos, nulabilidad e indices) en verde.
- No hay workflow de GitHub por la regla de `.github/workflows/README.md`; la suite hace de pipeline SQL.

## Plan maestro: fase 3 (privacidad minima)

Fase 3: PASS.
- SQL modulo `database/privacidad/` (esquema, seeds, validacion) sobre `NEROSERP`.
- API `api/privacidad`: cookies publicas, documentos pendientes/vigentes, aceptacion con hash e IP; bloqueo 403 `documentos_pendientes` tras cambio de clave obligatorio.
- BFF: `/cookies` (anon), `/legal/aceptar`, middleware y `/sesion/legal/aceptar`, claims legales, enlace en login; textos es/en/pt.
- Pruebas: 5 nuevas (API, servicio, `/cookies` en navegador) y ajustes de login con aceptacion legal; suite 80/80 PASS.

Despliegue local/NEROSERP (usuario): tras compatibilidad V0002, `apply --modulo privacidad` (ver `database/README.md`).

## Plan maestro: fase 4 (tenant / organizacion)

Fase 4: PASS.
- SQL modulo `database/organizacion/` (esquema `organizacion`, tenant NEROS-TEST, validacion) en base `NEROSORG`.
- `Neros.Organization.Api` + `Neros.Organization.Persistence`: correspondencia publica, contexto con regional desde JWT.
- Contratos `Neros.Contracts/Organizacion`; cabecera `X-Neros-Company-Id`.
- Gateway: puente sesion opaca → JWT de vida corta (PEM `Identity:BridgeSigningKeyPem`), ruta anonima de correspondencia, politica JWT Organization en `/context`.
- API compatibilidad: `GET api/empresas/{id}` para verificar membresia sin registrar entrada (puente).
- Pruebas: `OrganizacionTenantTests` (correspondencia, puente punta a punta, rechazo sin mapping) + regresion Gateway/Organization; suite 83/83 PASS.

Despliegue local: `apply --modulo organizacion --base NEROSORG --crear-base` (ver `database/README.md` y `database/organizacion/README.md`). Configurar `ConnectionStrings:Organization` y clave PEM del puente en Gateway y Organization.

Pendiente opcional BFF: enviar `X-Neros-Company-Id` desde `ClienteNeros` cuando hay empresa activa.

## Plan maestro: fase 26 (busqueda D-17b)

Fase 26: PASS.
- SQL `busqueda` V0001–V0002: indice `NEROSSEARCH` con `DocumentoIndice` (ACL, tombstone, version), `EventoIndexacion`, `CheckpointIngesta`, `EnlaceVista360`, outbox/inbox.
- Dominio `Busqueda`: ACL antes de paginar, tombstones, version de indice, proyeccion desde eventos, consulta keyset, frescura por checkpoint, enlaces vista 360.
- Permisos `BUSQUEDA.INDICE.CONSULTAR|MONITOREAR|RECONSTRUIR`.
- Contratos `Neros.Contracts.Busqueda`.
- Pruebas de dominio, SQL y permisos.

Pendiente opcional: motor full-text externo si el volumen lo exige.

## Transversal: D-03 persistencia + Search API + indexador (2026-09-26)

PASS.
- SQL inventario V0003; contabilidad V0004–V0005 (`MapeoRolContable`, comprobante automatico).
- Consumidor transaccional: inbox + efectos de dominio + escritores en la misma transaccion.
- `DespachadorOutboxModulos` (Ventas, Compras, Nomina, Facturacion).
- `EscritorIndexacionBusquedaSql` + cola `neros.busqueda.indexacion` en worker.
- `Neros.Search.Api` + gateway YARP.
- Pruebas: `IntegracionPersistenciaSqlTests` (comprobante), `IndexacionBusquedaSqlTests`, `IntegracionOutboxInboxSqlTests` (multi-modulo), E2E/search (20/20 filtro Integracion+Busqueda+SearchApi).

## Plan maestro: fase 25 (BI / analitica D-17a)

Fase 25: PASS.
- SQL `analitica` V0001–V0002: warehouse `NEROSANALYTICS` con marcas de agua, `EventoIngesta`, `HechoOperativo`, indicadores y outbox/inbox local.
- Dominio `Analitica`: linaje, ingesta (duplicados/tardios), proyeccion desde eventos de integracion, motor de indicadores (ventas, compras, nomina).
- Permisos `ANALITICA.INDICADOR.CONSULTAR|CONFIGURAR`, `ANALITICA.INGESTA.MONITOREAR`.
- Contratos publicos `Neros.Contracts.Analitica`.
- Pruebas de dominio, SQL y permisos.

Pendiente: worker `analitica.ingesta` persistente en SQL, API/dashboard, ETL batch.

## Plan maestro: fase 24 (integracion E2E / cierre D-03)

Fase 24: PASS.
- Dominio: `ProcesadorInboxIntegracion`, `ManejadorEventosIntegracion`, `MotorEnrutamientoIntegracion`; flujo recepcion dual inventario+contabilidad; nomina liquidada → reglas contables; inbox consulta previa a orden por `MessageId`.
- `Neros.Messaging.Sql`: deserializador de payload, `ConsumidorIntegracionPersistente`, registro de consumidores.
- RabbitMQ: recepcion por polling (`IEventBusReceptor`) y serde de sobres.
- Worker: `ServicioConsumoIntegracion` (colas `neros.inventario.*`, `neros.contabilidad.posting`) ademas del despacho outbox.
- Contrato `PayrollLiquidationPosted.v1`; validacion SQL `database/integracion/validation/002_consumidores_e2e.sql`.
- Pruebas: `IntegracionE2ETests` (cadena outbox→bus→consumidor, duplicados, desorden, recepcion dual); 11/11 PASS filtro Integracion.

Pendiente opcional: push consumer async en lugar de polling si el volumen lo exige.

## Plan maestro: fase 23 (proyectos)

Fase 23: PASS.
- SQL `proyectos` V0001–V0002: proyecto, fases, presupuesto por concepto, referencias a documentos de otros modulos, outbox/inbox.
- Dominio `Proyectos`: estados, validacion de imputacion, saldo vs presupuesto, variacion/ejecucion (§54).
- Permisos `PROYECTOS.PROYECTO.*`, `PROYECTOS.IMPUTACION.REGISTRAR`.
- Pruebas de dominio, SQL y permisos.

Pendiente: columnas ProyectoId en modulos origen, API/UI, eventos de imputacion automatica.

## Plan maestro: fase 22 (produccion)

Fase 22: PASS.
- SQL `produccion` V0001–V0002: lista de materiales, ruta, orden, movimientos (consumo, devolucion, terminado, merma), costos MP/MO/CIF, outbox/inbox.
- Dominio `Produccion`: explosion BOM, estados de orden, movimientos con topes, costo unitario terminado (§54).
- Permisos segregados: liberar orden y configurar BOM (Administrador); movimientos (Operador).
- Pruebas de dominio, SQL y permisos.

Pendiente: reservas/consumos en inventario vía eventos, API/UI, subensambles multi-nivel.

## Plan maestro: fase 21 (presupuesto)

Fase 21: PASS.
- SQL `presupuesto` V0001–V0002: centro de costo, version, lineas por cuenta/periodo, ejecucion (real), outbox/inbox.
- Dominio `Presupuesto`: variacion Real - Presupuesto, ejecucion % con N/A si presupuesto 0 (§54), estados de version.
- Permisos segregados: gestionar (Operador) vs aprobar version (Administrador).
- Pruebas de indicadores, SQL y permisos.

Pendiente: alimentacion automatica del real desde contabilidad, API/UI.

## Plan maestro: fase 20 (activos fijos)

Fase 20: PASS.
- SQL `activos` V0001–V0002: categoria, activo fijo, depreciacion por periodo, eventos inmutables, outbox/inbox.
- Dominio `Activos`: depreciacion lineal §54, deterioro, maquinas de estado activo y depreciacion contabilizada.
- Permisos `ACTIVOS.ACTIVO.*`, `ACTIVOS.DEPRECIACION.CALCULAR|CONTABILIZAR` (contabilizar solo Administrador).
- Pruebas de dominio, SQL y permisos.

Pendiente: API/UI, enlace contable por eventos, otros metodos de depreciacion.

## Plan maestro: fase 19 (nomina legal y electronica Colombia)

Fase 19: PASS (base tecnica; validacion humana de especialista y habilitacion DIAN produccion pendientes).
- SQL `nomina` V0003 + semilla S0001: paquete legal, afiliaciones, snapshot SS/retencion, nomina electronica, intentos DIAN.
- Dominio `Nomina/Legal`: paquete aprobado, aportes SS y retencion parametrizados; integracion en orquestador con deducciones legales.
- Dominio `Nomina/Electronica`: flujo borrador→validado, puerto DIAN sandbox, precondicion liquidacion contabilizada.
- Permisos `NOMINA.LEGAL.*`, `NOMINA.ELECTRONICO.*` (aprobar legal solo Administrador).
- Pruebas legales, electronicas, SQL y permisos.

Pendiente: XML nomina real, firma, produccion DIAN, UI, aprobacion de paquete en aplicacion.

## Plan maestro: fase 18 (nomina base)

Fase 18: PASS.
- SQL `nomina` V0001–V0002: empleado (tercero + datos laborales), contrato, conceptos, periodo, liquidacion, snapshot, lineas, outbox/inbox.
- Dominio `Nomina`: motores contractual, laboral, matematico, tributario y seguridad social separados; orquestador; snapshot §53; maquina de estados con inmutabilidad al contabilizar.
- Permisos segregados: calcular (Operador) vs contabilizar (Administrador); conceptos configurables solo Admin.
- Pruebas de dominio, SQL y permisos.

Pendiente: API/UI, PII/HCM, evento NOMINA_LIQUIDADA persistente, reglas legales Colombia (fase 19).

## Plan maestro: fase 17 (facturacion electronica Colombia)

Fase 17: PASS (base tecnica; habilitacion DIAN real y validacion de especialista pendientes).
- SQL `facturacion` V0001–V0002: numeracion fiscal, comercial/fiscal/electronico separados, snapshot legal, idempotencia, intentos de transmision, configuracion emisor, outbox/inbox.
- Dominio `Facturacion`: maquina de estados §50, numeracion §52, snapshot §51, idempotencia §56, puerto `IEnvioDocumentoElectronicoDian` y flujo sandbox.
- Permisos `FACTURACION.DOCUMENTO.*`, `FACTURACION.NUMERACION.CONFIGURAR`, `FACTURACION.ELECTRONICO.CONSULTAR`.
- Pruebas de dominio y despliegue SQL.

Pendiente: UBL/XML real, firma con certificado en secret store, conector produccion DIAN, API/UI, enlace pedido→factura→CxC y eventos contables.

## Plan maestro: fase 16 (contabilidad avanzada)

Fase 16: PASS.
- SQL `contabilidad` V0003: marco contable CO-GRUPO1/2/3, definicion de reportes, secciones, lineas, formulas y mapeo de cuentas (§41).
- Dominio `Contabilidad/Reportes`: motor de formulas SUM, SUBTRACT, PERCENT, VARIATION, RATIO; generacion y control Activo = Pasivo + Patrimonio con diferencia visible.
- Dominio `Contabilidad/DiferenciaCambio`: diferencia realizada al pagar y reexpresion no realizada (§45).
- Permisos `CONTABILIDAD.REPORTE.CONSULTAR|CONFIGURAR`.
- Pruebas de controles §41, indicadores N/A, reportes y FX.

Pendiente: seeds de reportes estandar, UI de reporting, asientos automaticos por diferencia en cambio.

## Plan maestro: fase 15 (tesoreria)

Fase 15: PASS.
- SQL modulo `database/tesoreria/`: cuentas caja/banco, movimientos, transferencias, extracto bancario, conciliacion y enlaces.
- Dominio `Neros.Domain/Tesoreria`: saldo por movimientos, transferencias pareadas, conciliacion libro vs extracto y cierre sin diferencia.
- Permisos `TESORERIA.CUENTA.*`, `TESORERIA.MOVIMIENTO.REGISTRAR`, `TESORERIA.CONCILIACION.*` (gestionar conciliacion solo Administrador).
- Pruebas de dominio, conciliacion y despliegue SQL.

Despliegue: `apply --modulo tesoreria --base NEROSTESORERIA --crear-base`.

Pendiente: servicio API, enlace automatico cartera↔tesoreria, eventos contables, importacion de extractos.

## Plan maestro: fase 14 (CxC y CxP)

Fase 14: PASS.
- SQL modulo `database/cartera/`: documentos CxC/CxP, cuotas, movimientos inmutables cargo/abono, pagos, aplicaciones y proyeccion de saldo.
- Dominio `Neros.Domain/Cartera`: saldo = cargos - abonos (§43), aplicaciones con anticipo, reconstruccion vs proyeccion.
- Permisos `CARTERA.CXC.*`, `CARTERA.CXP.*`, `CARTERA.PAGO.REGISTRAR`.
- Pruebas de invariantes, ejemplo 1.000.000 del plan y despliegue SQL.

Despliegue: `apply --modulo cartera --base NEROSCARTERA --crear-base`.

Pendiente: servicio API, enlace factura compra/venta, tesoreria (fase 15), diferencia en cambio (§45).

## Plan maestro: fase 13 (broker e integracion)

Fase 13: PASS.
- SQL `V0002__integracion_outbox_inbox.sql` en ventas, inventario, compras y contabilidad (§55).
- `Neros.Messaging.Sql`: outbox con lease, despachador e inbox idempotente.
- `Neros.Messaging.RabbitMQ`: `IEventBus` sobre exchange topic `neros.integracion`.
- Worker `Neros.Integration.Worker`: despacho periodico de outbox (base ventas configurable).
- Dominio `Neros.Domain/Integracion`: pedido→reserva, recepcion→entrada inventario, recepcion→reglas contables; ordenador de version ante desorden.
- Contratos `TiposEventoIntegracion`.
- Compose `deploy/development/docker-compose.rabbitmq.yml`.
- Pruebas: flujos, outbox/inbox SQL (duplicado, reproceso), broker opcional (`NEROS_REQUIRE_BROKER_TESTS=1`).

Pendiente (heredado a fase 24+): reserva/movimiento/comprobante persistidos; outbox multi-modulo en el worker. Consumidores RabbitMQ: ver fase 24.

## Plan maestro: fase 12 (compras)

Fase 12: PASS.
- SQL modulo `database/compras/`: orden de compra (borrador, aprobada, recibida parcial/total), lineas con acumulado recibido, recepciones, snapshot de proveedor e impuestos al aprobar.
- Dominio `Neros.Domain/Compras`: totales §40, aprobacion con segregacion creador/aprobador (§47), recepciones parciales y estado de orden.
- Permisos `COMPRAS.ORDEN.*` (aprobar solo Administrador) y `COMPRAS.RECEPCION.*`.
- Pruebas de dominio, segregacion, recepciones y despliegue SQL.

Despliegue: `apply --modulo compras --base NEROSCOMPRAS --crear-base`.

Pendiente: servicio API, movimiento de inventario al recibir (D-03), factura proveedor y CxP.

## Plan maestro: fase 11 (ventas)

Fase 11: PASS.
- SQL modulo `database/ventas/`: cotizacion y pedido (borrador, confirmado, anulado), lineas, impuestos por linea, snapshot de cliente e impuestos al confirmar.
- Dominio `Neros.Domain/Ventas`: totales via `MotorImpuestos` (§40), confirmacion con snapshot (§36), maquina de estados y pedido desde cotizacion confirmada (§46).
- Permisos `VENTAS.COTIZACION.*` y `VENTAS.PEDIDO.*`.
- Pruebas de dominio, alineacion §40 y despliegue SQL.

Despliegue: `apply --modulo ventas --base NEROSVENTAS --crear-base`.

Pendiente: servicio API, numeracion al emitir (§52), despacho/factura/CxC, reserva de inventario (D-03).

## Plan maestro: fase 10 (inventario)

Fase 10: PASS.
- SQL modulo `database/inventario/`: bodega, producto, periodo, existencia (rowversion), movimientos inmutables.
- Dominio `Neros.Domain/Inventario`: promedio ponderado (§49), recosteo cronologico, kardex reconstruido (§42), periodo cerrado, negativos opcionales, `LibroInventario` con bloqueo para concurrencia.
- Permisos `INVENTARIO.MOVIMIENTO.*`.
- Pruebas: costeo, retroactivo, kardex, concurrencia y SQL deploy.

Despliegue: `apply --modulo inventario --base NEROSINV --crear-base`.

Pendiente: API/servicio, reservas/disponible, capas FIFO, job de recosteo persistente.

## Plan maestro: fase 9 (contabilidad base)

Fase 9: PASS.
- SQL modulo `database/contabilidad/`: ejercicio, periodo, cuenta, tipo comprobante, comprobante, movimiento, documento origen; CHECK de lado unico en movimientos.
- Dominio `Neros.Domain/Contabilidad`: partida doble, saldos y balance de comprobacion (§34), ecuacion contable (§33), cierre/reapertura/reversion (§35), `MotorContabilizacion` para `VENTA_FACTURADA` (§37) sin numeros de cuenta en codigo.
- Permisos iniciales `CONTABILIDAD.*` en catalogo por rol.
- Pruebas de invariantes y despliegue SQL.

Despliegue: `apply --modulo contabilidad --base NEROSCONT --crear-base`.

Pendiente: servicio API, cierre de resultados automatico, reglas versionadas en SQL, trigger/SP de periodo cerrado.

## Plan maestro: fase 8 (impuestos)

Fase 8: PASS.
- SQL modulo `database/impuestos/`: jurisdiccion, impuesto, tarifa con vigencia, concepto, `VersionPublicada` + `VersionRegla` inmutables.
- Dominio `Neros.Domain/Impuestos`: `MotorImpuestos` puro (sin E/S), version inmutable, formulas §39 (base x tarifa, incluido, cascada, retencion) y totales §40.
- Contratos `Neros.Contracts/Impuestos` para publicacion futura.
- Pruebas: ejemplo IVA 19% del plan §37, lineas y documento con valor a pagar, cascada, SQL deploy.

Despliegue: `apply --modulo impuestos --base NEROSIMPUESTOS --crear-base`.

Pendiente: servicio API de publicacion, integracion Ventas/Compras en proceso, motor leyendo version desde SQL.

## Plan maestro: fase 7 (terceros)

Fase 7: PASS.
- SQL modulo `database/terceros/`: maestro con `SYSTEM_VERSIONING`, identificaciones, roles y cuentas bancarias de proveedor.
- Dominio: validacion de identificacion Colombia (NIT con digito, CC).
- `Neros.Terceros.Api` + `Neros.Terceros.Persistence`: CRUD, busqueda paginada, historial temporal.
- Permisos `TERCEROS.TERCERO.*` por rol de empresa; puente de sesion en gateway hacia JWT `neros.terceros`.
- Ruta gateway `/api/v1/terceros/*`; pruebas de dominio, SQL, API (CRUD, validacion rechazada, historial, 403 rol Consulta).

Despliegue: `apply --modulo terceros --base NEROSTERCEROS --crear-base`. Configurar `ConnectionStrings:Terceros` y `Services:Terceros` en gateway.

Pendiente: UI Blazor del modulo, cuentas bancarias expuestas con permiso restringido, integracion con paquetes de localizacion SQL.

## Plan maestro: fase 6 (globalizacion)

Fase 6: PASS.
- Dominio `Neros.Domain/Globalizacion`: `Dinero`, `CatalogoIso`, `MotorRedondeo`, `PoliticaRedondeo`, `ConversionMoneda`, `FechasNegocio`.
- SQL modulo `database/globalizacion/`: catalogos ISO, politicas, tasas, paquete `GENERICO`, `cumplimiento.ReglaNormativa` (POR_VERIFICAR); **V0002** catalogos transversales (zona horaria, unidad, tipo identificacion).
- API `GET api/globalizacion/catalogos` en host compatibilidad (`ConnectionStrings:Globalizacion` o misma base en pruebas).
- Pruebas: dominio, SQL (V0002), API catalogos.

Despliegue: `apply --modulo globalizacion --base NEROSGLOBAL --crear-base`.

## Plan maestro: fase 5 (design system)

Fase 5: PASS.
- Dashboard `/home` (§59): tarjetas de modulos desde `CatalogoModulos`, cuenta/seguridad (ultimo acceso, sesiones, clave temporal), actividad reciente, panel admin global.
- Navegacion lateral por modulos (disponibles vs en preparacion).
- Componentes compartidos: `NerosDataGrid` (paginacion servidor, cargando/vacio), `EstadoCargando`, `EstadoVacio`, `PaginaEstado`.
- Administracion de usuarios migrada a `NerosDataGrid`.
- Pruebas navegador: capturas dashboard es/en/pt (1440 y 390, claro/oscuro) y grid de usuarios; `npm run css:build` PASS.

## Plan maestro: fase 2 (cuenta y seguridad)

Fase 2: PASS.
- SQL `V0002__cuenta_sesiones_auditoria.sql` + validacion 002: `DebeCambiarClave`, datos y revocacion de sesiones, `auditoria.Evento` ledger append-only.
- API: `api/cuenta` (cambio de clave, sesiones propias) y `api/admin/usuarios` (listado paginado, restablecer, desbloquear) con politicas por permiso; bloqueo 403 `cambio_clave_requerido` con clave temporal; rate limit por sesion en cambio de clave.
- Permisos iniciales `MODULO.RECURSO.ACCION` (plataforma solo para AdministradorGlobal; catalogo por rol de empresa preparado).
- Gateway: rutas `/api/cuenta` y `/api/admin/usuarios`; cabeceras informativas de IP y agente.
- BFF: menu de usuario, `/cuenta/clave`, `/cuenta/sesiones`, `/administracion/usuarios`, redireccion obligatoria, textos es/en/pt.
- Hallazgo corregido en pruebas: recargar la pagina tras restablecer reenviaba el POST y generaba otra clave; la pagina ahora reemplaza la entrada del historial.

## Completed gates

I-01 revalidada: referencia Microsoft.AspNetCore.OpenApi ausente y ambos paquetes OpenAPI ausentes en assets de API/Tests. No es una nueva auditoria de todas las dependencias.

D-01: PASS. Correspondencia PRUEBA -> NEROS-TEST autorizada expresamente, validate contra SQL exit 0 sin cambios de datos. Suite 37/37 PASS, cuatro pruebas de mapping incluyen manifiesto estable, multiples tenants, empresa futura sin correspondencia y rechazo de CompanyId como TenantId. Build de once proyectos PASS (5.9 s). Contratos, threat model, proveedores, revocacion/fail closed, administrador global, recuperacion y estrategia de corte registrados. No se certifica persistencia de tenant ni subsistemas futuros por este gate.

D-02: PASS. [Gateway/BFF y Organization](D02_GATEWAY_ORGANIZATION.md) implementados sin SQL nuevo. Suite completa 41/41 PASS, cero omitidas; diez regresiones de acceso pasan ahora por Gateway con SQL temporal y navegador. Cuatro pruebas Gateway/Organization cubren contexto firmado, headers descartados, health, traza entre hosts, upstream detenido, Identity indisponible y concurrencia 32/33. Build trece proyectos PASS, NuGet sin vulnerabilidades conocidas en fuente oficial. No habilita broker, base Organization ni proveedor OIDC productivo.

## Pending gates

D-05–D-18 (productivo): extraccion definitiva de identidad/organizacion, OIDC (D-06), UI operativa por modulo, habilitacion DIAN/nomina con especialista, despliegue multi-base en produccion. D-03 ya no bloquea integracion tecnica base.

## Current blockers

Bloqueo contractual resuelto por autorizacion expresa, no por inferencia del nombre. [Manifiesto estable](../../deploy/development/company-tenant-map.json): NEROS-TEST, TenantId f70c8608-2f4a-4d79-90b8-30f1b569bd17 -> PRUEBA, CompanyId 42336010-6818-443b-9166-d03ab1754478 preservado. Sin grupo inventado, fallback o asignacion automatica futura. Snapshot local anterior bajo obj es evidencia historica, no la fuente aprobada. Nuevas empresas sin mapping detienen validacion. Tenant dedicado a pruebas no equivale a infraestructura dedicada ya desplegada.

Comprobacion de entorno al entrar a D-03: sin docker/podman/rabbitmqctl/rabbitmq-server/erl en PATH, sin servicios Docker/RabbitMQ, sin Docker en ruta habitual, cero listeners AMQP 5671/5672 y cero variables Rabbit/Broker/AMQP. No hay broker configurado/accesible para el gate. Se necesita habilitar Docker/RabbitMQ local o proporcionar acceso a un broker de pruebas mediante configuracion segura. No instalar con elevacion interactiva ni poner credenciales en chat/repositorio. No se han solicitado ni utilizado recursos cloud/productivos. No usar simulacion en memoria como sustituto del gate de persistencia.

## Important decisions

Plataforma distribuida SaaS, bases y propietarios independientes. Tenant -> BusinessGroup -> Company -> Branch, multiples empresas por tenant, mapping explicito, mismos contratos para shared/sharded/dedicated. Usuario confirma AdministradorGlobal con acceso a todo: concesion de plataforma vigente, tenant seleccionado y auditoria, no eliminar filtros tenant ni confiar en headers. Revocacion vigente/fail closed aprobados, sin ventana de permisos obsoletos. API, BFF, Organization y Gateway consumen foundation; Gateway:BaseUrl es opt-in compatible para no romper el arranque anterior. Mensajeria hoy son contratos, no almacenamiento durable. Organization tecnico valida JWT/contexto, no sustituye membresias vigentes de D-05.

## Migrations

Fase 1: `NEROSERP` sin modificar. En modo solo lectura, `plan` muestra `V0001` pendiente de baseline y `validate` pasa sin problemas; el baseline queda para que lo ejecute el usuario (ver `database/README.md`). Una base huerfana `NerosTests_11dbff84...` de una ejecucion anterior (20:35) sigue en localhost sin eliminar.

Anterior: NEROSERP y cuentas existentes no modificadas. Herramienta tools/Neros.Organization.Migration con snapshot/inspect/validate: SELECT unicamente, snapshots locales CreateNew sin sobrescribir. Manifiesto aprobado versionable en deploy/development/company-tenant-map.json: datos de desarrollo explicitos, no registros ya migrados. Cuatro pruebas cubren IDs estables, multiples empresas/tenants, grupos cruzados, duplicados e inventario cambiado. D-05 debe persistir TenantId antes de generar datos nuevos de negocio; permisos/jobs/eventos/Outbox/Inbox/archivos/Search/Analytics lo conservan en sus respectivos gates.

## Tests

Fase 6 (2026-09-26):
- Suite completa: 96/96 PASS, cero omitidas. Nuevas: `GlobalizacionDominioTests`, `GlobalizacionSqlTests`.
- `dotnet build Neros.slnx` PASS.

Fase 5 (2026-09-26):
- Suite completa: 85/85 PASS, cero omitidas. Nuevas: `DesignSystemTests` (dashboard i18n + grid admin).
- `dotnet build Neros.slnx` PASS; `npm run css:build` PASS.

Fase 4 (2026-09-26):
- Suite completa: 83/83 PASS, cero omitidas. Nuevas: correspondencia publica via Gateway, puente sesion→contexto con tenant/regional, rechazo empresa sin mapping.
- `dotnet build Neros.slnx` PASS.

Fase 3 (2026-09-25):
- Suite completa: 80/80 PASS, cero omitidas. Nuevas: cookies publicas, bloqueo sin aceptar, persistencia de aceptacion, servicio directo y pagina `/cookies` en navegador.
- `dotnet build Neros.slnx` PASS; `npm run css:build` PASS.

Fase 2 (2026-09-25):
- Suite completa: 75/75 PASS, cero omitidas. Nuevas: 5 de API (revocacion y renovacion al cambiar clave, rechazos auditados, 403 para no administradores, clave temporal obligatoria, listado y desbloqueo, sesiones propias y ajenas), 2 unitarias (catalogo de permisos y clave temporal) y 1 de navegador con capturas (menu, restablecimiento, cambio obligatorio, sesiones). La de deriva EF ignora columnas ocultas del ledger.
- `dotnet build Neros.slnx` PASS con 0 errores; `npm run css:build` PASS.

Fase 1 (2026-09-25):
- Suite completa: 67/67 PASS, cero omitidas. Son 42 anteriores y 25 nuevas: 24 del runner, entre unitarias e integracion SQL (idempotencia, script modificado, rollback de fallo, repetible, validacion, baseline, destructivos), y 1 de deriva EF.
- `dotnet build Neros.slnx` PASS con 0 errores.
- `npm run css:build` PASS.
- `graphify update .` PASS parcial: 1834 nodos y 2449 aristas; los `.sql` no se analizan por falta de `tree_sitter_sql`.

I-01: PASS estructural. Suite completa final: 41/41 PASS, cero omitidas (diez regresiones SQL/navegador por Gateway, veintitres foundation, cuatro mapping y cuatro Gateway/Organization). HTTP Kestrel: codigos 400/401/403/404/409/412/429/503, excepcion JSON y Accept HTML sin detalle privado en respuesta/log capturado, correlacion, GET retries/POST unico/timeout, JWT issuer/audience/firma/exp, scope/permiso/tenant, readiness y spans de hosts reales. Hash: escala/orden equivalentes; rechaza precision perdida/underflow/overflow. NuGet: trece proyectos sin vulnerabilidades conocidas en fuente oficial consultada. No se ha probado broker/collector remoto, carga ERP ni redaccion universal de todos los adaptadores.

Comandos ejecutados: dotnet test tests/Neros.Tests/Neros.Tests.csproj --no-restore --tl:off -v minimal; dotnet list Neros.slnx package --vulnerable --include-transitive --no-restore --source https://api.nuget.org/v3/index.json; dotnet build Neros.slnx --no-restore -v minimal. Build final PASS: trece proyectos, 5.8 s. Graphify update PASS parcial: grafo de codigo 1418 nodos/1769 aristas; no parser SQL, siete archivos sin nodos y semantica documental no regenerada. No confundir grafo con validacion SQL.

Diagnosticos de documentos/codigo nuevos sin errores reportados. El editor conserva alerta obsoleta Microsoft.OpenApi 2.0.0 en csproj de tests; ambos paquetes OpenAPI ausentes en assets actuales API/Tests, restore/build/auditoria oficial correctos. Informes locales excluidos por git check-ignore. Verificacion final: cero procesos Neros y cero listeners en puertos de desarrollo conocidos. Sin commit/push ni cambios a datos/cuentas existentes.

## Next action

Usuario: desplegar modulos SQL pendientes (`terceros` en `NEROSTERCEROS`, etc.) segun entorno. Siguiente hito: fase 11 (ventas: cotizacion y pedido). En paralelo, habilitar broker para D-03. Mantener TenantId obligatorio y autorizacion por ambito al implementar permisos/jobs/eventos/Outbox/Inbox/archivos/Search/Analytics, sin afirmar que el manifiesto implementa esos subsistemas. NEROS-TEST es un dato de desarrollo, no politica de asignacion. Ningun servidor Neros queda abierto al finalizar.