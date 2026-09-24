# ADR-0001: evolucion incremental a monolito modular

Fecha: 2026-09-13. Estado: **SUPERSEDED** el 2026-09-13 por [ADR-0002: Distributed Modular ERP Platform](ADR-0002-distributed-modular-platform.md). Alcance: nueva solucion Neros.

Este documento conserva la decision anterior como historial, no como instruccion vigente. El destino aprobado es una plataforma distribuida con servicios autonomos, datos exclusivos y despliegue independiente. No se mantiene como destino un backend, DbContext o base transaccional compartidos. El contenido siguiente describe exclusivamente la decision sustituida.

## Contexto

La aplicacion tiene dos hosts web, capas separadas, SQL Server y un flujo de autenticacion/empresas probado. No hay modulos transaccionales ni evidencia de necesidades de despliegue independiente. Una reescritura o distribucion inmediata aumentaria fallos parciales, operacion y migracion sin resolver las brechas actuales. Ver [diagnostico](../NEROS_ERP_ARCHITECTURE_ASSESSMENT.md).

## Decision

Conservar proyectos, tablas e identidad actuales. Agregar limites de negocio con carpetas por modulo dentro de Domain/Application/Contracts/Persistence/API/UI y contratos explicitos. Cada modulo posee reglas y escrituras; Shared permanece pequeno. La API compone infraestructura y no asume reglas extensas. Blazor usa API, nunca DbContext.

Empezar con una base OLTP y transacciones locales para efectos financieros que requieran atomicidad. Introducir outbox y worker durable con el primer caso real que necesite entrega externa o trabajo masivo. No introducir microservicios, Kafka, Kubernetes, event sourcing, multiples bases o CQRS completo por defecto.

## Alternativas

- Reescritura: descartada; la base funciona y no hay incompatibilidad fundamental demostrada.
- Microservicios inmediatos: descartados; sin equipos/despliegues/cargas independientes que compensen su costo.
- Continuar en servicios transversales gigantes: descartado; concentra reglas y hace fragil el crecimiento.
- Proyecto separado por cada modulo desde hoy: aplazado; comenzar con limites logicos y separar compilacion cuando haya un modulo real que lo justifique.

## Consecuencias

Reutilizar autenticacion y UI; permitir pruebas de integracion y atomicidad SQL simples. A cambio, disciplinar propiedad de datos y dependencias: no escribir tablas de otros modulos directamente ni compartir entidades EF con UI. Introducir tests de limites al aparecer el segundo modulo real. Un DbContext compartido inicialmente no autoriza un modelo de dominio universal.

La extraccion futura requiere motivo medido: escalado independiente, aislamiento de fallos, disponibilidad, equipo propietario o integracion. Antes de extraer, definir contrato versionado, consistencia, idempotencia, trazabilidad, migracion y operacion.

## Compatibilidad y verificacion

Este ADR no mueve archivos ni datos. Implementaciones posteriores usan scripts expand/backfill/contract y conservan IDs. Probar flujos actuales y permisos en cada entrega. La primera iniciativa solo retira una dependencia vulnerable inactiva; no simula una modularizacion completa. Ver [roadmap](../NEROS_ERP_IMPLEMENTATION_ROADMAP.md) y [arquitectura objetivo](../NEROS_ERP_TARGET_ARCHITECTURE.md).