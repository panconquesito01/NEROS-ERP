/*
===============================================================================
Neros ERP
Script        : V0001__esquema_inventario.sql
Modulo        : inventario
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Bodegas, movimientos inmutables, existencia y periodos (plan fase 10, §42, §49).
Dependencias  : Ninguna
Objetos       : inventario.Bodega, Producto, PeriodoInventario, Existencia, MovimientoInventario
Motivo        : Base del modulo Inventario.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_inventario.sql
===============================================================================
*/

CREATE SCHEMA [inventario];
GO

CREATE TABLE [inventario].[Bodega]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Bodega] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(20) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [PermitirExistenciasNegativas] bit NOT NULL CONSTRAINT [DF_Bodega_Negativos] DEFAULT (0),
    [Activa] bit NOT NULL CONSTRAINT [DF_Bodega_Activa] DEFAULT (1)
);
GO

CREATE UNIQUE INDEX [UX_Bodega_Empresa_Codigo]
    ON [inventario].[Bodega] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [inventario].[Producto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Producto] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Producto_Activo] DEFAULT (1)
);
GO

CREATE UNIQUE INDEX [UX_Producto_Empresa_Codigo]
    ON [inventario].[Producto] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [inventario].[PeriodoInventario]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PeriodoInventario] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [FechaInicio] date NOT NULL,
    [FechaFin] date NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_PeriodoInventario_Estado] DEFAULT ('Abierto'),
    CONSTRAINT [CK_PeriodoInventario_Estado] CHECK ([Estado] IN ('Abierto', 'Cerrado')),
    CONSTRAINT [CK_PeriodoInventario_Rango] CHECK ([FechaFin] >= [FechaInicio])
);
GO

CREATE TABLE [inventario].[Existencia]
(
    [ProductoId] uniqueidentifier NOT NULL,
    [BodegaId] uniqueidentifier NOT NULL,
    [Cantidad] decimal(28, 10) NOT NULL CONSTRAINT [DF_Existencia_Cantidad] DEFAULT (0),
    [ValorTotal] decimal(28, 10) NOT NULL CONSTRAINT [DF_Existencia_Valor] DEFAULT (0),
    [CostoPromedio] decimal(28, 10) NOT NULL CONSTRAINT [DF_Existencia_Costo] DEFAULT (0),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Existencia] PRIMARY KEY ([ProductoId], [BodegaId]),
    CONSTRAINT [FK_Existencia_Producto] FOREIGN KEY ([ProductoId]) REFERENCES [inventario].[Producto] ([Id]),
    CONSTRAINT [FK_Existencia_Bodega] FOREIGN KEY ([BodegaId]) REFERENCES [inventario].[Bodega] ([Id])
);
GO

CREATE TABLE [inventario].[MovimientoInventario]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MovimientoInventario] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [ProductoId] uniqueidentifier NOT NULL,
    [BodegaId] uniqueidentifier NOT NULL,
    [Fecha] date NOT NULL,
    [Secuencia] int NOT NULL,
    [Tipo] varchar(20) NOT NULL,
    [EsEntrada] bit NOT NULL,
    [Cantidad] decimal(28, 10) NOT NULL,
    [CostoUnitarioEntrada] decimal(28, 10) NULL,
    [CostoUnitarioAplicado] decimal(28, 10) NOT NULL,
    [ValorTotal] decimal(28, 10) NOT NULL,
    [MarcadoAjusteNegativo] bit NOT NULL CONSTRAINT [DF_Movimiento_AjusteNeg] DEFAULT (0),
    [ModuloOrigen] varchar(40) NULL,
    [DocumentoOrigenId] uniqueidentifier NULL,
    CONSTRAINT [FK_Movimiento_Producto] FOREIGN KEY ([ProductoId]) REFERENCES [inventario].[Producto] ([Id]),
    CONSTRAINT [FK_Movimiento_Bodega] FOREIGN KEY ([BodegaId]) REFERENCES [inventario].[Bodega] ([Id]),
    CONSTRAINT [CK_Movimiento_Cantidad] CHECK ([Cantidad] > 0),
    CONSTRAINT [CK_Movimiento_Tipo] CHECK ([Tipo] IN ('Entrada', 'Salida', 'Ajuste', 'Transferencia', 'Devolucion', 'Produccion'))
);
GO

CREATE UNIQUE INDEX [UX_Movimiento_Orden]
    ON [inventario].[MovimientoInventario] ([ProductoId], [BodegaId], [Fecha], [Secuencia]);
GO

CREATE INDEX [IX_Movimiento_Producto_Bodega_Fecha]
    ON [inventario].[MovimientoInventario] ([ProductoId], [BodegaId], [Fecha], [Secuencia]);
GO
