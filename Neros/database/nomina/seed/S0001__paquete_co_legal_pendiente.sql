/*
===============================================================================
Neros ERP
Script        : S0001__paquete_co_legal_pendiente.sql
Modulo        : nomina
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Plantilla CO-LEGAL en PendienteEspecialista (no usar en calculo productivo).
Dependencias  : V0003__legal_colombia_nomina_electronica.sql
Objetos       : INSERT nomina.PaqueteLegalNomina
Motivo        : Referencia de tarifas para habilitacion y revision legal.
Impacto       : Datos semilla.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : N/A
===============================================================================
*/

IF NOT EXISTS (SELECT 1 FROM [nomina].[PaqueteLegalNomina] WHERE [Codigo] = 'CO-LEGAL' AND [Version] = N'2026-01-PLANTILLA')
INSERT INTO [nomina].[PaqueteLegalNomina]
(
    [Id], [Codigo], [Version], [VigenciaDesde], [VigenciaHasta],
    [Smmlv], [TarifaSaludEmpleadoPct], [TarifaPensionEmpleadoPct],
    [TarifaSaludEmpleadorPct], [TarifaPensionEmpleadorPct], [TarifaArlEmpleadorPct], [TarifaCajaEmpleadorPct],
    [UmbralRetencionFuente], [TarifaRetencionFuentePct], [EstadoAprobacion]
)
VALUES
(
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'CO-LEGAL', N'2026-01-PLANTILLA', '2026-01-01', '2026-12-31',
    1423500.0000000000, 4.0000, 4.0000, 8.5000, 12.0000, 0.5220, 4.0000,
    950000.0000000000, 0.0000, 'PendienteEspecialista'
);
GO
