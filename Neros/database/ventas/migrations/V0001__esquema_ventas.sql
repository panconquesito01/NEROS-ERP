/*
===============================================================================
Neros ERP
Script        : V0001__esquema_ventas.sql
Modulo        : ventas
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Cotizacion y pedido con snapshot de tercero e impuestos (plan fase 11, §36, §40, §46).
Dependencias  : Ninguna
Objetos       : ventas.Cotizacion, CotizacionLinea, CotizacionLineaImpuesto, Pedido, PedidoLinea, PedidoLineaImpuesto
Motivo        : Base del modulo Ventas (cotizacion y pedido).
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_ventas.sql
===============================================================================
*/

CREATE SCHEMA [ventas];
GO

CREATE TABLE [ventas].[Cotizacion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Cotizacion] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Numero] varchar(30) NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Cotizacion_Estado] DEFAULT ('Borrador'),
    [FechaDocumento] date NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [TasaCambio] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cotizacion_Tasa] DEFAULT (1),
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
    [VersionSoftware] nvarchar(40) NULL,
    [PrecioUnitarioIncluyeImpuesto] bit NOT NULL CONSTRAINT [DF_Cotizacion_PrecioIncluye] DEFAULT (0),
    [AnticiposAplicados] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cotizacion_Anticipos] DEFAULT (0),
    [Subtotal] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cotizacion_Subtotal] DEFAULT (0),
    [TotalImpuestos] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cotizacion_Impuestos] DEFAULT (0),
    [TotalRetenciones] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cotizacion_Retenciones] DEFAULT (0),
    [Total] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cotizacion_Total] DEFAULT (0),
    [ValorAPagar] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cotizacion_ValorAPagar] DEFAULT (0),
    [ConfirmadoEnUtc] datetime2(3) NULL,
    [AnuladoEnUtc] datetime2(3) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Cotizacion_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_Cotizacion_Estado] CHECK ([Estado] IN ('Borrador', 'Confirmado', 'Anulado')),
    CONSTRAINT [CK_Cotizacion_Tasa] CHECK ([TasaCambio] > 0),
    CONSTRAINT [CK_Cotizacion_Anticipos] CHECK ([AnticiposAplicados] >= 0)
);
GO

CREATE INDEX [IX_Cotizacion_Empresa_Estado]
    ON [ventas].[Cotizacion] ([TenantId], [EmpresaId], [Estado], [FechaDocumento] DESC);
GO

CREATE TABLE [ventas].[CotizacionLinea]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_CotizacionLinea] PRIMARY KEY,
    [CotizacionId] uniqueidentifier NOT NULL,
    [LineaNumero] int NOT NULL,
    [ProductoReferenciaId] uniqueidentifier NULL,
    [Descripcion] nvarchar(240) NOT NULL,
    [Cantidad] decimal(28, 10) NOT NULL,
    [PrecioUnitario] decimal(28, 10) NOT NULL,
    [DescuentoLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_CotizacionLinea_Descuento] DEFAULT (0),
    [Bruto] decimal(28, 10) NOT NULL CONSTRAINT [DF_CotizacionLinea_Bruto] DEFAULT (0),
    [BaseNeta] decimal(28, 10) NOT NULL CONSTRAINT [DF_CotizacionLinea_Base] DEFAULT (0),
    [TotalImpuestosLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_CotizacionLinea_Imp] DEFAULT (0),
    [TotalRetencionesLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_CotizacionLinea_Ret] DEFAULT (0),
    CONSTRAINT [FK_CotizacionLinea_Cotizacion] FOREIGN KEY ([CotizacionId]) REFERENCES [ventas].[Cotizacion] ([Id]),
    CONSTRAINT [CK_CotizacionLinea_Cantidad] CHECK ([Cantidad] > 0),
    CONSTRAINT [CK_CotizacionLinea_Precio] CHECK ([PrecioUnitario] >= 0),
    CONSTRAINT [CK_CotizacionLinea_Descuento] CHECK ([DescuentoLinea] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_CotizacionLinea_Numero]
    ON [ventas].[CotizacionLinea] ([CotizacionId], [LineaNumero]);
GO

CREATE TABLE [ventas].[CotizacionLineaImpuesto]
(
    [CotizacionLineaId] uniqueidentifier NOT NULL,
    [CodigoImpuesto] varchar(30) NOT NULL,
    [BaseGravable] decimal(28, 10) NOT NULL,
    [Tarifa] decimal(28, 10) NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [EsRetencion] bit NOT NULL,
    CONSTRAINT [PK_CotizacionLineaImpuesto] PRIMARY KEY ([CotizacionLineaId], [CodigoImpuesto]),
    CONSTRAINT [FK_CotizacionLineaImpuesto_Linea] FOREIGN KEY ([CotizacionLineaId]) REFERENCES [ventas].[CotizacionLinea] ([Id])
);
GO

CREATE TABLE [ventas].[Pedido]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Pedido] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [CotizacionOrigenId] uniqueidentifier NULL,
    [Numero] varchar(30) NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Pedido_Estado] DEFAULT ('Borrador'),
    [FechaDocumento] date NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [TasaCambio] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pedido_Tasa] DEFAULT (1),
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
    [VersionSoftware] nvarchar(40) NULL,
    [PrecioUnitarioIncluyeImpuesto] bit NOT NULL CONSTRAINT [DF_Pedido_PrecioIncluye] DEFAULT (0),
    [AnticiposAplicados] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pedido_Anticipos] DEFAULT (0),
    [Subtotal] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pedido_Subtotal] DEFAULT (0),
    [TotalImpuestos] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pedido_Impuestos] DEFAULT (0),
    [TotalRetenciones] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pedido_Retenciones] DEFAULT (0),
    [Total] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pedido_Total] DEFAULT (0),
    [ValorAPagar] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pedido_ValorAPagar] DEFAULT (0),
    [ConfirmadoEnUtc] datetime2(3) NULL,
    [AnuladoEnUtc] datetime2(3) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Pedido_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_Pedido_Estado] CHECK ([Estado] IN ('Borrador', 'Confirmado', 'Anulado')),
    CONSTRAINT [CK_Pedido_Tasa] CHECK ([TasaCambio] > 0),
    CONSTRAINT [CK_Pedido_Anticipos] CHECK ([AnticiposAplicados] >= 0),
    CONSTRAINT [FK_Pedido_CotizacionOrigen] FOREIGN KEY ([CotizacionOrigenId]) REFERENCES [ventas].[Cotizacion] ([Id])
);
GO

CREATE INDEX [IX_Pedido_Empresa_Estado]
    ON [ventas].[Pedido] ([TenantId], [EmpresaId], [Estado], [FechaDocumento] DESC);
GO

CREATE TABLE [ventas].[PedidoLinea]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PedidoLinea] PRIMARY KEY,
    [PedidoId] uniqueidentifier NOT NULL,
    [LineaNumero] int NOT NULL,
    [ProductoReferenciaId] uniqueidentifier NULL,
    [Descripcion] nvarchar(240) NOT NULL,
    [Cantidad] decimal(28, 10) NOT NULL,
    [PrecioUnitario] decimal(28, 10) NOT NULL,
    [DescuentoLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_PedidoLinea_Descuento] DEFAULT (0),
    [Bruto] decimal(28, 10) NOT NULL CONSTRAINT [DF_PedidoLinea_Bruto] DEFAULT (0),
    [BaseNeta] decimal(28, 10) NOT NULL CONSTRAINT [DF_PedidoLinea_Base] DEFAULT (0),
    [TotalImpuestosLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_PedidoLinea_Imp] DEFAULT (0),
    [TotalRetencionesLinea] decimal(28, 10) NOT NULL CONSTRAINT [DF_PedidoLinea_Ret] DEFAULT (0),
    CONSTRAINT [FK_PedidoLinea_Pedido] FOREIGN KEY ([PedidoId]) REFERENCES [ventas].[Pedido] ([Id]),
    CONSTRAINT [CK_PedidoLinea_Cantidad] CHECK ([Cantidad] > 0),
    CONSTRAINT [CK_PedidoLinea_Precio] CHECK ([PrecioUnitario] >= 0),
    CONSTRAINT [CK_PedidoLinea_Descuento] CHECK ([DescuentoLinea] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_PedidoLinea_Numero]
    ON [ventas].[PedidoLinea] ([PedidoId], [LineaNumero]);
GO

CREATE TABLE [ventas].[PedidoLineaImpuesto]
(
    [PedidoLineaId] uniqueidentifier NOT NULL,
    [CodigoImpuesto] varchar(30) NOT NULL,
    [BaseGravable] decimal(28, 10) NOT NULL,
    [Tarifa] decimal(28, 10) NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [EsRetencion] bit NOT NULL,
    CONSTRAINT [PK_PedidoLineaImpuesto] PRIMARY KEY ([PedidoLineaId], [CodigoImpuesto]),
    CONSTRAINT [FK_PedidoLineaImpuesto_Linea] FOREIGN KEY ([PedidoLineaId]) REFERENCES [ventas].[PedidoLinea] ([Id])
);
GO
