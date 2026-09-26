/*
===============================================================================
Neros ERP
Script        : V0001__esquema_compras.sql
Modulo        : compras
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Orden de compra y recepcion con snapshot e impuestos (plan fase 12, §36, §40, §47).
Dependencias  : Ninguna
Objetos       : compras.OrdenCompra, OrdenCompraLinea, OrdenCompraLineaImpuesto, RecepcionCompra, RecepcionCompraLinea
Motivo        : Base del modulo Compras (orden y recepcion).
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_compras.sql
===============================================================================
*/

CREATE SCHEMA [compras];
GO

CREATE TABLE [compras].[OrdenCompra]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_OrdenCompra] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Numero] varchar(30) NULL,
    [Estado] varchar(25) NOT NULL CONSTRAINT [DF_OrdenCompra_Estado] DEFAULT ('Borrador'),
    [FechaDocumento] date NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [TasaCambio] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompra_Tasa] DEFAULT (1),
    [ProveedorTerceroId] uniqueidentifier NOT NULL,
    [ProveedorTipoIdentificacion] varchar(20) NULL,
    [ProveedorNumeroIdentificacion] varchar(40) NULL,
    [ProveedorRazonSocial] nvarchar(200) NULL,
    [ProveedorDireccion] nvarchar(240) NULL,
    [ProveedorCiudad] nvarchar(80) NULL,
    [ProveedorCorreo] varchar(320) NULL,
    [ProveedorResponsabilidadesFiscales] nvarchar(500) NULL,
    [ImpuestosVersionPublicadaId] uniqueidentifier NULL,
    [ImpuestosVersionNumero] int NULL,
    [VersionSoftware] nvarchar(40) NULL,
    [PrecioUnitarioIncluyeImpuesto] bit NOT NULL CONSTRAINT [DF_OrdenCompra_PrecioIncluye] DEFAULT (0),
    [AnticiposAplicados] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompra_Anticipos] DEFAULT (0),
    [Subtotal] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompra_Subtotal] DEFAULT (0),
    [TotalImpuestos] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompra_Impuestos] DEFAULT (0),
    [TotalRetenciones] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompra_Retenciones] DEFAULT (0),
    [Total] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompra_Total] DEFAULT (0),
    [ValorAPagar] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompra_ValorAPagar] DEFAULT (0),
    [CreadoPorUsuarioId] uniqueidentifier NOT NULL,
    [AprobadoPorUsuarioId] uniqueidentifier NULL,
    [AprobadoEnUtc] datetime2(3) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_OrdenCompra_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_OrdenCompra_Estado] CHECK ([Estado] IN ('Borrador', 'Aprobada', 'RecibidaParcial', 'RecibidaTotal')),
    CONSTRAINT [CK_OrdenCompra_Tasa] CHECK ([TasaCambio] > 0),
    CONSTRAINT [CK_OrdenCompra_Anticipos] CHECK ([AnticiposAplicados] >= 0)
);
GO

CREATE INDEX [IX_OrdenCompra_Empresa_Estado]
    ON [compras].[OrdenCompra] ([TenantId], [EmpresaId], [Estado], [FechaDocumento] DESC);
GO

CREATE TABLE [compras].[OrdenCompraLinea]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_OrdenCompraLinea] PRIMARY KEY,
    [OrdenCompraId] uniqueidentifier NOT NULL,
    [LineaNumero] int NOT NULL,
    [ProductoReferenciaId] uniqueidentifier NULL,
    [Descripcion] nvarchar(240) NOT NULL,
    [CantidadPedida] decimal(28, 10) NOT NULL,
    [CantidadRecibidaAcumulada] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompraLinea_Recibida] DEFAULT (0),
    [PrecioUnitario] decimal(28, 10) NOT NULL,
    [DescuentoLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompraLinea_Descuento] DEFAULT (0),
    [Bruto] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompraLinea_Bruto] DEFAULT (0),
    [BaseNeta] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompraLinea_Base] DEFAULT (0),
    [TotalImpuestosLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompraLinea_Imp] DEFAULT (0),
    [TotalRetencionesLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenCompraLinea_Ret] DEFAULT (0),
    CONSTRAINT [FK_OrdenCompraLinea_Orden] FOREIGN KEY ([OrdenCompraId]) REFERENCES [compras].[OrdenCompra] ([Id]),
    CONSTRAINT [CK_OrdenCompraLinea_CantidadPedida] CHECK ([CantidadPedida] > 0),
    CONSTRAINT [CK_OrdenCompraLinea_Recibida] CHECK ([CantidadRecibidaAcumulada] >= 0),
    CONSTRAINT [CK_OrdenCompraLinea_RecibidaNoExcede] CHECK ([CantidadRecibidaAcumulada] <= [CantidadPedida]),
    CONSTRAINT [CK_OrdenCompraLinea_Precio] CHECK ([PrecioUnitario] >= 0),
    CONSTRAINT [CK_OrdenCompraLinea_Descuento] CHECK ([DescuentoLinea] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_OrdenCompraLinea_Numero]
    ON [compras].[OrdenCompraLinea] ([OrdenCompraId], [LineaNumero]);
GO

CREATE TABLE [compras].[OrdenCompraLineaImpuesto]
(
    [OrdenCompraLineaId] uniqueidentifier NOT NULL,
    [CodigoImpuesto] varchar(30) NOT NULL,
    [BaseGravable] decimal(28, 10) NOT NULL,
    [Tarifa] decimal(28, 10) NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [EsRetencion] bit NOT NULL,
    CONSTRAINT [PK_OrdenCompraLineaImpuesto] PRIMARY KEY ([OrdenCompraLineaId], [CodigoImpuesto]),
    CONSTRAINT [FK_OrdenCompraLineaImpuesto_Linea] FOREIGN KEY ([OrdenCompraLineaId]) REFERENCES [compras].[OrdenCompraLinea] ([Id])
);
GO

CREATE TABLE [compras].[RecepcionCompra]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_RecepcionCompra] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [OrdenCompraId] uniqueidentifier NOT NULL,
    [Numero] varchar(30) NULL,
    [FechaRecepcion] date NOT NULL,
    [BodegaReferenciaId] uniqueidentifier NULL,
    [RegistradoPorUsuarioId] uniqueidentifier NOT NULL,
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_RecepcionCompra_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_RecepcionCompra_Orden] FOREIGN KEY ([OrdenCompraId]) REFERENCES [compras].[OrdenCompra] ([Id])
);
GO

CREATE INDEX [IX_RecepcionCompra_Orden]
    ON [compras].[RecepcionCompra] ([OrdenCompraId], [FechaRecepcion] DESC);
GO

CREATE TABLE [compras].[RecepcionCompraLinea]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_RecepcionCompraLinea] PRIMARY KEY,
    [RecepcionCompraId] uniqueidentifier NOT NULL,
    [OrdenCompraLineaId] uniqueidentifier NOT NULL,
    [CantidadRecibida] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_RecepcionCompraLinea_Recepcion] FOREIGN KEY ([RecepcionCompraId]) REFERENCES [compras].[RecepcionCompra] ([Id]),
    CONSTRAINT [FK_RecepcionCompraLinea_OrdenLinea] FOREIGN KEY ([OrdenCompraLineaId]) REFERENCES [compras].[OrdenCompraLinea] ([Id]),
    CONSTRAINT [CK_RecepcionCompraLinea_Cantidad] CHECK ([CantidadRecibida] > 0)
);
GO

CREATE UNIQUE INDEX [UX_RecepcionCompraLinea_Unica]
    ON [compras].[RecepcionCompraLinea] ([RecepcionCompraId], [OrdenCompraLineaId]);
GO
