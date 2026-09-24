# ADR-0003: foundation de desarrollo y contratos de borde

Fecha: 2026-09-13. Estado: ACCEPTED para implementacion local bajo la autorizacion de ejecucion del roadmap. Complementa [ADR-0002](ADR-0002-distributed-modular-platform.md), no aprueba despliegue productivo ni capacidad de escala.

## Decisiones

- OIDC: OpenIddict con ASP.NET Core Identity para D-06, manteniendo credenciales/IDs existentes y protocolo en libreria probada. No degraded mode ni protocolo OAuth artesanal. Authorization code + PKCE para BFF; client credentials para workloads; no password grant general. OpenIddict no implementa por si solo MFA/recovery ni UI: esos controles siguen en Identity. Ensayar persistencia, revocacion y certificados antes del corte. La [integracion oficial](https://documentation.openiddict.com/integrations/aspnet-core.html) declara soporte .NET 10.
- Mantener revocacion actual durante compatibilidad: ningun cache de permisos que cambie su efecto; futuros tokens tenant no autorizan escritura solo por conservar claims antiguos. El contrato de decision fresca/revocacion se implementa en D-05/D-06 antes de recursos reales, con fail closed. No adoptar todavia la tolerancia de 60 s propuesta en arquitectura.
- D-02 usa YARP 2.3.0 para routing, no negocio. JWT Bearer valida firma, emisor, audiencia, vigencia y scopes; token opaco actual solo en rutas de compatibilidad que conservan la validacion SQL actual, nunca convertido en JWT.
- D-03 usa RabbitMQ y cliente oficial .NET como primer adaptador de IEventBus, con confirms, mandatory, ACK manual y colas durables. No Kafka. ASB productivo requiere ADR de hosting, no es dependencia de desarrollo. Las [confirmaciones](https://www.rabbitmq.com/docs/confirms) del productor no confirman procesamiento del consumidor.
- D-04 prioriza Aspire AppHost para Visual Studio y nombres logicos; no requiere Aspire como plataforma productiva. Evaluar ejecucion al llegar al gate: al iniciar D-01 no hay docker/aspire/rabbitmqctl en PATH ni servicios Docker/RabbitMQ registrados. No simular broker ni marcar pruebas cloud/containers aprobadas.
- OpenTelemetry 1.18.0 y Microsoft.Extensions.Http.Resilience 10.10.0 fijados. OTLP opt-in por OTEL_EXPORTER_OTLP_ENDPOINT; destino/credenciales por entorno. Sin endpoint no fingir exportacion remota. HTTP instrumentado y spans exportados en pruebas locales. Retries solo de consultas seguras, presupuesto 10 s, intento 3 s, maximo dos retries y concurrencia 32.

## Alternativas y consecuencias

OpenIddict permite conservar Identity sin requerir cuenta cloud; Entra/Keycloak siguen siendo federacion futura si existe caso. Un servidor empaquetado externo reduce codigo de identidad pero introduce otra plataforma y migracion de credenciales. OpenIddict exige operar claves, consentimientos y revocacion correctamente; D-06 no queda aprobado por elegirlo.

ServiceDefaults contiene solo adaptadores ASP.NET/HTTP/seguridad tecnica. Messaging.Abstractions no depende de ASP.NET/EF/broker. No mover reglas, entidades, persistencia o contratos empresariales a estas bibliotecas. Versiones por servicio pueden divergir conservando contrato. No inventar Result universal sin consumidor.

## Verificacion y limites

Validacion inicial: build completo y 22/22 pruebas el 2026-09-13; ampliacion a 35/35 tras precision/HTTP/mapping. Cierre D-01: usuario autoriza PRUEBA -> NEROS-TEST, manifiesto explicito validado contra SQL, suite 37/37 y build de once proyectos correctos. JWT usa firmas RSA efimeras en pruebas, no autenticador que acepta todo. Dos hosts reales emiten spans OTel; no prueba de collector productivo. NuGet sin vulnerabilidades conocidas en fuente consultada. D-01 no autoriza saltar los gates de migracion o despliegue.

D-01 decide y prueba contratos de foundation; persistencia Outbox/Inbox, broker/crash/replay se verifican en D-03, y migraciones de usuarios en D-05/D-06. No se han creado bases o movido cuentas. Evidencia detallada en [estado de ejecucion](../execution/NEROS_EXECUTION_STATUS.md).