---
description: 'Planificar o implementar la migracion Neros del flujo Reempaque y Saldos Pendientes con Blazor, API, Application, Domain y SQL manual.'
mode: agent
---

# Reempaque Y Saldos Pendientes - Neros

Este prompt convierte el flujo legacy de Produccion en una feature nueva de Neros. El sistema anterior es referencia funcional, no arquitectura objetivo.

## Arquitectura obligatoria

- UI: componentes Blazor en `Neros.Blazor` con Tailwind CSS.
- API: endpoints en `Neros.Api`, delgados y autorizados.
- Casos de uso: `Neros.Application`.
- Reglas: `Neros.Domain`.
- DTOs: `Neros.Contracts`.
- Tipos transversales: `Neros.Shared`.
- Persistencia futura: infraestructura/persistence implementando puertos de Application.
- Base de datos: scripts SQL Server manuales en `database/scripts/`.

## No hacer

- No crear MVC Areas ni vistas `.cshtml` nuevas.
- No usar Bootstrap, jQuery, DataTables ni Select2.
- No usar DbContext desde Blazor.
- No copiar `Servicios/*`, `EQContext` o controladores del monolito como destino.
- No ejecutar cambios de base real sin autorizacion explicita.

## Preflight

1. Leer `AGENTS.md`, `CONVENTIONS.md` y skills relevantes.
2. Consultar Graphify si se permite ejecucion local; si no, leer `graphify-out/GRAPH_REPORT.md`.
3. Identificar comportamiento funcional legacy que se va a migrar.
4. Separar reglas de negocio, contratos, UI, API, datos, seguridad y validaciones.

## Objetivo funcional

Construir un flujo independiente para:

- Consultar saldos pendientes por empresa, orden, lote, producto, etapa y presentacion.
- Iniciar reempaque desde producto aprobado, semiterminado o empacado.
- Convertir presentacion/producto origen en destino con conversion valida.
- Registrar entrada, salida, diferencia, merma, material de empaque y unidades logisticas.
- Procesar parcial cuando la etapa lo permita.
- Mantener trazabilidad origen-destino, lote comercial e inventario.
- Consultar detalle e historico.
- Cerrar saldo residual con motivo, permiso y auditoria.
- Bloquear cierre de produccion si existen saldos obligatorios o inconsistencias.

## Reglas de negocio

- Definir una unica formula centralizada de saldo.
- Nunca mezclar datos entre empresas/tenants/sucursales.
- Rechazar cantidades <= 0 o superiores al saldo.
- Si la etapa no permite parcial, exigir saldo completo.
- Validar conversion en servidor; no confiar en factor enviado por UI.
- Recalcular salida esperada, diferencia, merma, tolerancia y saldo restante en servidor.
- Generar movimientos separados de salida e ingreso cuando aplique inventario.
- Mantener trazabilidad completa y auditable.
- Guardado/procesamiento debe ser atomico e idempotente.
- No borrar fisicamente documentos procesados.
- Cierre de saldo exige motivo, usuario, fecha y autorizacion.

## Diseno por capas

- Domain: value objects para saldo, cantidad, conversion, tolerancia y estados.
- Application: casos de uso `ConsultarSaldosPendientes`, `CrearReempaque`, `ProcesarReempaque`, `CerrarSaldo`, `ConsultarHistorico`.
- Contracts: requests/responses para filtros, detalle, creacion, procesamiento y cierre.
- Api: endpoints protegidos y delgados.
- Blazor: bandeja, formulario, detalle/historico y estados de validacion.
- Data: scripts SQL manuales para tablas, indices, constraints y auditoria cuando se implemente persistencia.

## Seguridad

- Permisos minimos: consultar, crear, procesar, cerrar/anular, ver historico.
- Validar permisos en API/Application, no solo en UI.
- Auditar procesamiento, cierre, anulacion y errores relevantes.
- No loggear payloads sensibles.

## Validacion

Cuando se permita ejecutar local:

```powershell
npm run css:build
dotnet build .\Neros.slnx -v minimal
graphify update .
```

Si no se permite ejecutar local, usar diagnosticos del editor y dejar estos comandos pendientes.

## Entrega

Reportar:

- Reglas implementadas.
- Archivos por capa.
- Contratos creados.
- Scripts SQL creados o pendientes.
- Validaciones ejecutadas o pendientes.
- Riesgos residuales.