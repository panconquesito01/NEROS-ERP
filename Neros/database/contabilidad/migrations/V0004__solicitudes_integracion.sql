/*
===============================================================================
Neros ERP
Script        : V0004__solicitudes_integracion.sql
Modulo        : contabilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Cola de contabilizacion solicitada por integracion (D-03).
Dependencias  : V0002__integracion_outbox_inbox.sql
Objetos       : contabilidad.SolicitudContabilizacionIntegracion
Motivo        : Persistir reglas generadas hasta comprobante manual/automatico.
Impacto       : Tabla nueva.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/004_solicitudes_integracion.sql
===============================================================================
*/

CREATE TABLE [contabilidad].[SolicitudContabilizacionIntegracion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_SolicitudContabilizacionIntegracion] PRIMARY KEY,
    [EventoIntegracionId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [OrigenModulo] varchar(40) NOT NULL,
    [OrigenAgregadoId] varchar(200) NOT NULL,
    [TipoEvento] varchar(120) NOT NULL,
    [LineasJson] nvarchar(max) NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_SolicitudContab_Estado] DEFAULT ('Pendiente'),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SolicitudContab_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_SolicitudContab_Estado] CHECK ([Estado] IN ('Pendiente', 'Contabilizado', 'Rechazado')),
    CONSTRAINT [CK_SolicitudContab_Lineas] CHECK (ISJSON([LineasJson]) = 1)
);
GO

CREATE UNIQUE INDEX [UX_SolicitudContab_Evento]
    ON [contabilidad].[SolicitudContabilizacionIntegracion] ([EventoIntegracionId]);
GO
