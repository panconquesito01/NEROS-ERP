/*
===============================================================================
Neros ERP
Script        : V0003__legal_colombia_nomina_electronica.sql
Modulo        : nomina
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Paquete legal Colombia, afiliaciones, aportes/retencion en snapshot y nomina electronica DIAN (plan fase 19, §50, §53).
Dependencias  : V0001__esquema_nomina_base.sql
Objetos       : nomina.PaqueteLegalNomina, AfiliacionEmpleado, LiquidacionSeguridadSocial, LiquidacionRetencionFuente, NominaElectronica, IntentoTransmisionNominaElectronica, ConfiguracionNominaElectronica
Motivo        : Nomina legal y electronica Colombia.
Impacto       : Tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : MEDIO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/003_legal_y_electronica.sql
===============================================================================
*/

CREATE TABLE [nomina].[PaqueteLegalNomina]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PaqueteLegalNomina] PRIMARY KEY,
    [Codigo] varchar(30) NOT NULL,
    [Version] nvarchar(40) NOT NULL,
    [VigenciaDesde] date NOT NULL,
    [VigenciaHasta] date NULL,
    [Smmlv] decimal(28, 10) NOT NULL,
    [TarifaSaludEmpleadoPct] decimal(8, 4) NOT NULL,
    [TarifaPensionEmpleadoPct] decimal(8, 4) NOT NULL,
    [TarifaSaludEmpleadorPct] decimal(8, 4) NOT NULL,
    [TarifaPensionEmpleadorPct] decimal(8, 4) NOT NULL,
    [TarifaArlEmpleadorPct] decimal(8, 4) NOT NULL,
    [TarifaCajaEmpleadorPct] decimal(8, 4) NOT NULL,
    [UmbralRetencionFuente] decimal(28, 10) NOT NULL,
    [TarifaRetencionFuentePct] decimal(8, 4) NOT NULL,
    [EstadoAprobacion] varchar(30) NOT NULL CONSTRAINT [DF_PaqueteLegal_Estado] DEFAULT ('Borrador'),
    [AprobadoPorEspecialistaEnUtc] datetime2(3) NULL,
    [NotasEspecialista] nvarchar(2000) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_PaqueteLegal_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [UQ_PaqueteLegal_Codigo_Version] UNIQUE ([Codigo], [Version]),
    CONSTRAINT [CK_PaqueteLegal_Estado] CHECK ([EstadoAprobacion] IN ('Borrador', 'PendienteEspecialista', 'Aprobado')),
    CONSTRAINT [CK_PaqueteLegal_Smmlv] CHECK ([Smmlv] > 0)
);
GO

CREATE TABLE [nomina].[AfiliacionEmpleado]
(
    [EmpleadoId] uniqueidentifier NOT NULL CONSTRAINT [PK_AfiliacionEmpleado] PRIMARY KEY,
    [CodigoEps] varchar(20) NULL,
    [CodigoAfp] varchar(20) NULL,
    [CodigoArl] varchar(20) NULL,
    [CodigoCajaCompensacion] varchar(20) NULL,
    [ClaseRiesgoArl] varchar(10) NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Afiliacion_Actualizado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_Afiliacion_Empleado] FOREIGN KEY ([EmpleadoId]) REFERENCES [nomina].[Empleado] ([Id])
);
GO

CREATE TABLE [nomina].[LiquidacionSeguridadSocial]
(
    [LiquidacionNominaId] uniqueidentifier NOT NULL CONSTRAINT [PK_LiquidacionSeguridadSocial] PRIMARY KEY,
    [PaqueteLegalNominaId] uniqueidentifier NOT NULL,
    [BaseCotizacion] decimal(28, 10) NOT NULL,
    [AporteSaludEmpleado] decimal(28, 10) NOT NULL,
    [AportePensionEmpleado] decimal(28, 10) NOT NULL,
    [AporteSaludEmpleador] decimal(28, 10) NOT NULL,
    [AportePensionEmpleador] decimal(28, 10) NOT NULL,
    [AporteArlEmpleador] decimal(28, 10) NOT NULL,
    [AporteCajaEmpleador] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_LiqSS_Liquidacion] FOREIGN KEY ([LiquidacionNominaId]) REFERENCES [nomina].[LiquidacionNomina] ([Id]),
    CONSTRAINT [FK_LiqSS_Paquete] FOREIGN KEY ([PaqueteLegalNominaId]) REFERENCES [nomina].[PaqueteLegalNomina] ([Id])
);
GO

CREATE TABLE [nomina].[LiquidacionRetencionFuente]
(
    [LiquidacionNominaId] uniqueidentifier NOT NULL CONSTRAINT [PK_LiquidacionRetencionFuente] PRIMARY KEY,
    [PaqueteLegalNominaId] uniqueidentifier NOT NULL,
    [BaseRetencion] decimal(28, 10) NOT NULL,
    [ImporteRetencion] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_LiqRet_Liquidacion] FOREIGN KEY ([LiquidacionNominaId]) REFERENCES [nomina].[LiquidacionNomina] ([Id]),
    CONSTRAINT [FK_LiqRet_Paquete] FOREIGN KEY ([PaqueteLegalNominaId]) REFERENCES [nomina].[PaqueteLegalNomina] ([Id])
);
GO

CREATE TABLE [nomina].[ConfiguracionNominaElectronica]
(
    [EmpresaId] uniqueidentifier NOT NULL CONSTRAINT [PK_ConfigNominaElectronica] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [NitEmisor] varchar(20) NOT NULL,
    [RazonSocial] nvarchar(200) NOT NULL,
    [AmbienteDian] varchar(20) NOT NULL CONSTRAINT [DF_ConfigNomElec_Ambiente] DEFAULT ('Habilitacion'),
    [CertificadoSecretoId] varchar(120) NULL,
    [SoftwareId] varchar(120) NULL,
    [VersionSoftware] nvarchar(40) NOT NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ConfigNomElec_Actualizado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_ConfigNomElec_Ambiente] CHECK ([AmbienteDian] IN ('Habilitacion', 'Produccion'))
);
GO

CREATE TABLE [nomina].[NominaElectronica]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_NominaElectronica] PRIMARY KEY,
    [LiquidacionNominaId] uniqueidentifier NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_NomElec_Estado] DEFAULT ('Borrador'),
    [Cune] varchar(96) NULL,
    [HashXml] varchar(128) NULL,
    [TrackIdDian] varchar(120) NULL,
    [MensajeAutoridad] nvarchar(2000) NULL,
    [ValidadoEnUtc] datetime2(3) NULL,
    [RechazadoEnUtc] datetime2(3) NULL,
    [AnuladoEnUtc] datetime2(3) NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_NomElec_Actualizado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_NominaElectronica_Liquidacion] FOREIGN KEY ([LiquidacionNominaId]) REFERENCES [nomina].[LiquidacionNomina] ([Id]),
    CONSTRAINT [UQ_NominaElectronica_Liquidacion] UNIQUE ([LiquidacionNominaId]),
    CONSTRAINT [CK_NomElec_Estado] CHECK ([Estado] IN (
        'Borrador', 'Generado', 'Firmado', 'Enviado', 'Validado', 'Rechazado', 'Entregado', 'Anulado'))
);
GO

CREATE TABLE [nomina].[IntentoTransmisionNominaElectronica]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_IntentoTransmisionNominaElectronica] PRIMARY KEY,
    [NominaElectronicaId] uniqueidentifier NOT NULL,
    [NumeroIntento] int NOT NULL,
    [IdempotencyKey] varchar(120) NOT NULL,
    [AmbienteDian] varchar(20) NOT NULL,
    [CodigoRespuesta] varchar(40) NULL,
    [Exito] bit NOT NULL,
    [Detalle] nvarchar(2000) NULL,
    [OcurridoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_IntentoNomElec_Ocurrido] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_IntentoNomElec_NominaElectronica] FOREIGN KEY ([NominaElectronicaId]) REFERENCES [nomina].[NominaElectronica] ([Id]),
    CONSTRAINT [CK_IntentoNomElec_Ambiente] CHECK ([AmbienteDian] IN ('Habilitacion', 'Produccion'))
);
GO

CREATE UNIQUE INDEX [UX_IntentoNomElec_Idempotency]
    ON [nomina].[IntentoTransmisionNominaElectronica] ([NominaElectronicaId], [IdempotencyKey]);
GO
