---
description: 'Crear script SQL Server manual para Neros con idempotencia, rollback y seguridad.'
mode: agent
---

# Crear Script SQL - Neros

Crear scripts manuales en `database/scripts/` para cambios de base de datos.

## Reglas

- SQL Server.
- Idempotencia cuando sea razonable.
- Sin datos sensibles.
- Sin passwords, tokens ni connection strings.
- Documentar rollback, mitigacion o compensacion.
- Parametrizar consultas en codigo consumidor.
- No depender de EF migrations salvo aprobacion explicita.

## Nombre sugerido

```text
database/scripts/yyyyMMdd-HHmm-<modulo>-<cambio>.sql
```

## Contenido esperado

- Proposito.
- Precondiciones.
- Cambios DDL/DML.
- Indices/constraints si aplican.
- Datos de referencia si aplican.
- Verificacion.
- Rollback o mitigacion.

## Validacion

Indicar la consulta de verificacion y riesgos de ejecucion. No ejecutar contra base real sin autorizacion explicita.