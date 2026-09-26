/*
===============================================================================
Neros ERP
Script        : V0002__integracion_outbox_inbox.sql
Modulo        : ventas
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Outbox e Inbox locales (plan §55, fase 13).
Dependencias  : V0001__esquema_ventas.sql
Objetos       : integracion.MensajeSalida, integracion.MensajeEntrada
Motivo        : Despacho de eventos de ventas al broker.
Impacto       : Esquema integracion nuevo.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/002_integracion.sql
===============================================================================
*/

IF SCHEMA_ID(N'integracion') IS NULL
    EXEC(N'CREATE SCHEMA [integracion]');
GO

CREATE TABLE [integracion].[MensajeSalida]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MensajeSalida] PRIMARY KEY,
    [EventoId] uniqueidentifier NOT NULL,
    [TipoEvento] varchar(120) NOT NULL,
    [VersionEvento] int NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NULL,
    [AgregadoId] varchar(200) NOT NULL,
    [OcurridoEnUtc] datetime2(3) NOT NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [CausationId] uniqueidentifier NULL,
    [Payload] nvarchar(max) NOT NULL,
    [PublicadoEnUtc] datetime2(3) NULL,
    [Intentos] int NOT NULL CONSTRAINT [DF_MensajeSalida_Intentos] DEFAULT (0),
    [UltimoError] nvarchar(2000) NULL,
    [LeaseHastaUtc] datetime2(3) NULL,
    [LeasePropietario] varchar(100) NULL,
    CONSTRAINT [UQ_MensajeSalida_EventoId] UNIQUE ([EventoId])
);
GO

CREATE INDEX [IX_MensajeSalida_Pendiente]
    ON [integracion].[MensajeSalida] ([PublicadoEnUtc], [LeaseHastaUtc])
    WHERE [PublicadoEnUtc] IS NULL;
GO

CREATE TABLE [integracion].[MensajeEntrada]
(
    [Consumidor] varchar(100) NOT NULL,
    [EventoId] uniqueidentifier NOT NULL,
    [RecibidoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_MensajeEntrada_Recibido] DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_MensajeEntrada] PRIMARY KEY ([Consumidor], [EventoId])
);
GO
