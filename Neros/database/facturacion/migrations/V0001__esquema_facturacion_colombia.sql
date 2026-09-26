/*
===============================================================================
Neros ERP
Script        : V0001__esquema_facturacion_colombia.sql
Modulo        : facturacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Documento comercial/fiscal/electronico, numeracion DIAN, snapshot legal e idempotencia (plan fase 17, §50–§52, §56).
Dependencias  : Ninguna
Objetos       : facturacion.NumeracionDocumento, DocumentoComercial, DocumentoFiscal, DocumentoElectronico, IntentoTransmision, ClaveIdempotencia, ConfiguracionEmisor
Motivo        : Base de facturacion electronica Colombia.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_facturacion.sql
===============================================================================
*/

CREATE SCHEMA [facturacion];
GO

CREATE TABLE [facturacion].[NumeracionDocumento]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_NumeracionDocumento] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoDocumento] varchar(30) NOT NULL,
    [Prefijo] varchar(10) NOT NULL,
    [Desde] bigint NOT NULL,
    [Hasta] bigint NOT NULL,
    [Actual] bigint NOT NULL,
    [VigenciaDesde] date NOT NULL,
    [VigenciaHasta] date NOT NULL,
    [Resolucion] nvarchar(120) NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Numeracion_Estado] DEFAULT ('Activo'),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_Numeracion_Rango] CHECK ([Desde] <= [Hasta]),
    CONSTRAINT [CK_Numeracion_Actual] CHECK ([Actual] >= [Desde] - 1 AND [Actual] <= [Hasta]),
    CONSTRAINT [CK_Numeracion_Vigencia] CHECK ([VigenciaDesde] <= [VigenciaHasta]),
    CONSTRAINT [CK_Numeracion_Estado] CHECK ([Estado] IN ('Activo', 'Agotado', 'Vencido', 'Inactivo'))
);
GO

CREATE UNIQUE INDEX [UX_Numeracion_Empresa_Tipo_Prefijo]
    ON [facturacion].[NumeracionDocumento] ([TenantId], [EmpresaId], [TipoDocumento], [Prefijo]);
GO

CREATE TABLE [facturacion].[ConfiguracionEmisor]
(
    [EmpresaId] uniqueidentifier NOT NULL CONSTRAINT [PK_ConfiguracionEmisor] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [NitEmisor] varchar(20) NOT NULL,
    [DigitoVerificacion] char(1) NULL,
    [RazonSocial] nvarchar(200) NOT NULL,
    [RegimenFiscal] nvarchar(80) NULL,
    [ResponsabilidadesFiscales] nvarchar(500) NULL,
    [AmbienteDian] varchar(20) NOT NULL CONSTRAINT [DF_ConfigEmisor_Ambiente] DEFAULT ('Habilitacion'),
    [CertificadoSecretoId] varchar(120) NULL,
    [SoftwareId] varchar(120) NULL,
    [PinSoftwareSecretoId] varchar(120) NULL,
    [VersionNormativa] nvarchar(40) NOT NULL,
    [VersionReglasTributarias] nvarchar(40) NOT NULL,
    [VersionSoftware] nvarchar(40) NOT NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ConfigEmisor_Actualizado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_ConfigEmisor_Ambiente] CHECK ([AmbienteDian] IN ('Habilitacion', 'Produccion'))
);
GO

CREATE TABLE [facturacion].[DocumentoComercial]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoComercial] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoDocumento] varchar(30) NOT NULL CONSTRAINT [DF_DocComercial_Tipo] DEFAULT ('FacturaVenta'),
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_DocComercial_Estado] DEFAULT ('Borrador'),
    [PedidoOrigenId] uniqueidentifier NULL,
    [FechaDocumento] date NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [TasaCambio] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComercial_Tasa] DEFAULT (1),
    [ClienteTerceroId] uniqueidentifier NOT NULL,
    [ClienteTipoIdentificacion] varchar(20) NULL,
    [ClienteNumeroIdentificacion] varchar(40) NULL,
    [ClienteRazonSocial] nvarchar(200) NULL,
    [ClienteDireccion] nvarchar(240) NULL,
    [ClienteCiudad] nvarchar(80) NULL,
    [ClienteCorreo] varchar(320) NULL,
    [ClienteResponsabilidadesFiscales] nvarchar(500) NULL,
    [ImpuestosVersionPublicadaId] uniqueidentifier NULL,
    [ImpuestosVersionNumero] int NULL,
    [Subtotal] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComercial_Subtotal] DEFAULT (0),
    [TotalImpuestos] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComercial_Imp] DEFAULT (0),
    [TotalRetenciones] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComercial_Ret] DEFAULT (0),
    [Total] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComercial_Total] DEFAULT (0),
    [EmitidoEnUtc] datetime2(3) NULL,
    [AnuladoEnUtc] datetime2(3) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_DocComercial_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_DocComercial_Estado] CHECK ([Estado] IN ('Borrador', 'Emitido', 'Anulado')),
    CONSTRAINT [CK_DocComercial_Tipo] CHECK ([TipoDocumento] IN ('FacturaVenta', 'NotaCredito', 'NotaDebito')),
    CONSTRAINT [CK_DocComercial_Tasa] CHECK ([TasaCambio] > 0)
);
GO

CREATE INDEX [IX_DocumentoComercial_Empresa_Estado]
    ON [facturacion].[DocumentoComercial] ([TenantId], [EmpresaId], [Estado], [FechaDocumento] DESC);
GO

CREATE TABLE [facturacion].[DocumentoComercialLinea]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoComercialLinea] PRIMARY KEY,
    [DocumentoComercialId] uniqueidentifier NOT NULL,
    [LineaNumero] int NOT NULL,
    [ProductoReferenciaId] uniqueidentifier NULL,
    [Descripcion] nvarchar(240) NOT NULL,
    [Cantidad] decimal(28, 10) NOT NULL,
    [PrecioUnitario] decimal(28, 10) NOT NULL,
    [DescuentoLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComLinea_Desc] DEFAULT (0),
    [BaseNeta] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComLinea_Base] DEFAULT (0),
    [TotalImpuestosLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_DocComLinea_Imp] DEFAULT (0),
    CONSTRAINT [FK_DocComLinea_Documento] FOREIGN KEY ([DocumentoComercialId]) REFERENCES [facturacion].[DocumentoComercial] ([Id]),
    CONSTRAINT [CK_DocComLinea_Cantidad] CHECK ([Cantidad] > 0)
);
GO

CREATE UNIQUE INDEX [UX_DocComLinea_Numero]
    ON [facturacion].[DocumentoComercialLinea] ([DocumentoComercialId], [LineaNumero]);
GO

CREATE TABLE [facturacion].[DocumentoFiscal]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoFiscal] PRIMARY KEY,
    [DocumentoComercialId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoDocumento] varchar(30) NOT NULL,
    [Prefijo] varchar(10) NOT NULL,
    [NumeroFiscal] bigint NOT NULL,
    [NumeroPresentacion] varchar(30) NOT NULL,
    [Resolucion] nvarchar(120) NOT NULL,
    [NumeracionDocumentoId] uniqueidentifier NOT NULL,
    [FechaExpedicionNegocio] date NOT NULL,
    [ExpedidoEnUtc] datetime2(3) NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [TasaCambio] decimal(28, 10) NOT NULL,
    [EmisorNit] varchar(20) NOT NULL,
    [EmisorRazonSocial] nvarchar(200) NOT NULL,
    [EmisorResponsabilidades] nvarchar(500) NULL,
    [AdquirenteTipoIdentificacion] varchar(20) NOT NULL,
    [AdquirenteNumeroIdentificacion] varchar(40) NOT NULL,
    [AdquirenteRazonSocial] nvarchar(200) NOT NULL,
    [AdquirenteDireccion] nvarchar(240) NULL,
    [VersionNormativa] nvarchar(40) NOT NULL,
    [VersionReglasTributarias] nvarchar(40) NOT NULL,
    [ImpuestosVersionPublicadaId] uniqueidentifier NULL,
    [ImpuestosVersionNumero] int NULL,
    [VersionSoftware] nvarchar(40) NOT NULL,
    [Subtotal] decimal(28, 10) NOT NULL,
    [TotalImpuestos] decimal(28, 10) NOT NULL,
    [TotalRetenciones] decimal(28, 10) NOT NULL,
    [Total] decimal(28, 10) NOT NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_DocFiscal_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_DocumentoFiscal_Comercial] FOREIGN KEY ([DocumentoComercialId]) REFERENCES [facturacion].[DocumentoComercial] ([Id]),
    CONSTRAINT [FK_DocumentoFiscal_Numeracion] FOREIGN KEY ([NumeracionDocumentoId]) REFERENCES [facturacion].[NumeracionDocumento] ([Id]),
    CONSTRAINT [UQ_DocumentoFiscal_Comercial] UNIQUE ([DocumentoComercialId])
);
GO

CREATE UNIQUE INDEX [UX_DocumentoFiscal_Empresa_Numero]
    ON [facturacion].[DocumentoFiscal] ([TenantId], [EmpresaId], [TipoDocumento], [Prefijo], [NumeroFiscal]);
GO

CREATE TABLE [facturacion].[DocumentoElectronico]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoElectronico] PRIMARY KEY,
    [DocumentoFiscalId] uniqueidentifier NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_DocElectronico_Estado] DEFAULT ('Borrador'),
    [Cufe] varchar(96) NULL,
    [HashXml] varchar(128) NULL,
    [TrackIdDian] varchar(120) NULL,
    [MensajeAutoridad] nvarchar(2000) NULL,
    [PdfGeneradoEnUtc] datetime2(3) NULL,
    [ValidadoEnUtc] datetime2(3) NULL,
    [RechazadoEnUtc] datetime2(3) NULL,
    [AnuladoEnUtc] datetime2(3) NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_DocElectronico_Actualizado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_DocumentoElectronico_Fiscal] FOREIGN KEY ([DocumentoFiscalId]) REFERENCES [facturacion].[DocumentoFiscal] ([Id]),
    CONSTRAINT [UQ_DocumentoElectronico_Fiscal] UNIQUE ([DocumentoFiscalId]),
    CONSTRAINT [CK_DocElectronico_Estado] CHECK ([Estado] IN (
        'Borrador', 'Generado', 'Firmado', 'Enviado', 'Validado', 'Rechazado', 'Entregado', 'Anulado'))
);
GO

CREATE INDEX [IX_DocumentoElectronico_Estado]
    ON [facturacion].[DocumentoElectronico] ([Estado], [ActualizadoEnUtc] DESC);
GO

CREATE TABLE [facturacion].[IntentoTransmision]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_IntentoTransmision] PRIMARY KEY,
    [DocumentoElectronicoId] uniqueidentifier NOT NULL,
    [NumeroIntento] int NOT NULL,
    [IdempotencyKey] varchar(120) NOT NULL,
    [AmbienteDian] varchar(20) NOT NULL,
    [CodigoRespuesta] varchar(40) NULL,
    [Exito] bit NOT NULL,
    [Detalle] nvarchar(2000) NULL,
    [OcurridoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_IntentoTransmision_Ocurrido] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_IntentoTransmision_Electronico] FOREIGN KEY ([DocumentoElectronicoId]) REFERENCES [facturacion].[DocumentoElectronico] ([Id]),
    CONSTRAINT [CK_IntentoTransmision_Ambiente] CHECK ([AmbienteDian] IN ('Habilitacion', 'Produccion'))
);
GO

CREATE UNIQUE INDEX [UX_IntentoTransmision_Idempotency]
    ON [facturacion].[IntentoTransmision] ([DocumentoElectronicoId], [IdempotencyKey]);
GO

CREATE TABLE [facturacion].[ClaveIdempotencia]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ClaveIdempotencia] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Operacion] varchar(60) NOT NULL,
    [Clave] varchar(120) NOT NULL,
    [HashSolicitud] varchar(128) NOT NULL,
    [RespuestaSerializada] nvarchar(max) NOT NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ClaveIdempotencia_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [UQ_ClaveIdempotencia_Empresa_Operacion_Clave] UNIQUE ([TenantId], [EmpresaId], [Operacion], [Clave])
);
GO
