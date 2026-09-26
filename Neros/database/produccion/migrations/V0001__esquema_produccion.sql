/*
===============================================================================
Neros ERP
Script        : V0001__esquema_produccion.sql
Modulo        : produccion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : BOM, ruta, orden de produccion, consumos, devoluciones, terminados y merma (plan fase 22, §54).
Dependencias  : Ninguna
Objetos       : produccion.ListaMateriales, ListaMaterialesLinea, RutaProduccion, OrdenProduccion, MovimientoProduccion, CostoOrdenProduccion
Motivo        : Base de manufactura.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_produccion.sql
===============================================================================
*/

CREATE SCHEMA [produccion];
GO

CREATE TABLE [produccion].[ListaMateriales]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ListaMateriales] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [ProductoTerminadoReferenciaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Version] int NOT NULL CONSTRAINT [DF_ListaMateriales_Version] DEFAULT (1),
    [CantidadBaseSalida] decimal(28, 10) NOT NULL CONSTRAINT [DF_ListaMateriales_CantBase] DEFAULT (1),
    [Activa] bit NOT NULL CONSTRAINT [DF_ListaMateriales_Activa] DEFAULT (1),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ListaMateriales_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_ListaMateriales_CantBase] CHECK ([CantidadBaseSalida] > 0)
);
GO

CREATE UNIQUE INDEX [UX_ListaMateriales_Empresa_Codigo_Version]
    ON [produccion].[ListaMateriales] ([TenantId], [EmpresaId], [Codigo], [Version]);
GO

CREATE TABLE [produccion].[ListaMaterialesLinea]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ListaMaterialesLinea] PRIMARY KEY,
    [ListaMaterialesId] uniqueidentifier NOT NULL,
    [LineaNumero] int NOT NULL,
    [ComponenteReferenciaId] uniqueidentifier NOT NULL,
    [CantidadPorBase] decimal(28, 10) NOT NULL,
    [EsSubensamble] bit NOT NULL CONSTRAINT [DF_LmLinea_Sub] DEFAULT (0),
    CONSTRAINT [FK_LmLinea_Lista] FOREIGN KEY ([ListaMaterialesId]) REFERENCES [produccion].[ListaMateriales] ([Id]),
    CONSTRAINT [CK_LmLinea_Cantidad] CHECK ([CantidadPorBase] > 0)
);
GO

CREATE UNIQUE INDEX [UX_LmLinea_Numero]
    ON [produccion].[ListaMaterialesLinea] ([ListaMaterialesId], [LineaNumero]);
GO

CREATE TABLE [produccion].[RutaProduccion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_RutaProduccion] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [ListaMaterialesId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [Activa] bit NOT NULL CONSTRAINT [DF_Ruta_Activa] DEFAULT (1),
    CONSTRAINT [FK_Ruta_ListaMateriales] FOREIGN KEY ([ListaMaterialesId]) REFERENCES [produccion].[ListaMateriales] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_RutaProduccion_Empresa_Codigo]
    ON [produccion].[RutaProduccion] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [produccion].[RutaProduccionOperacion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_RutaProduccionOperacion] PRIMARY KEY,
    [RutaProduccionId] uniqueidentifier NOT NULL,
    [Secuencia] int NOT NULL,
    [CentroTrabajoCodigo] varchar(30) NOT NULL,
    [Descripcion] nvarchar(240) NOT NULL,
    [HorasEstandar] decimal(28, 10) NOT NULL CONSTRAINT [DF_RutaOp_Horas] DEFAULT (0),
    CONSTRAINT [FK_RutaOp_Ruta] FOREIGN KEY ([RutaProduccionId]) REFERENCES [produccion].[RutaProduccion] ([Id]),
    CONSTRAINT [CK_RutaOp_Horas] CHECK ([HorasEstandar] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_RutaOp_Secuencia]
    ON [produccion].[RutaProduccionOperacion] ([RutaProduccionId], [Secuencia]);
GO

CREATE TABLE [produccion].[OrdenProduccion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_OrdenProduccion] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Numero] varchar(30) NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_OrdenProd_Estado] DEFAULT ('Planificada'),
    [ListaMaterialesId] uniqueidentifier NOT NULL,
    [RutaProduccionId] uniqueidentifier NULL,
    [CantidadPlanificada] decimal(28, 10) NOT NULL,
    [CantidadTerminada] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenProd_Terminada] DEFAULT (0),
    [CantidadMerma] decimal(28, 10) NOT NULL CONSTRAINT [DF_OrdenProd_Merma] DEFAULT (0),
    [FechaInicioPlan] date NOT NULL,
    [FechaFinPlan] date NULL,
    [ProyectoId] uniqueidentifier NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_OrdenProd_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_OrdenProd_Lista] FOREIGN KEY ([ListaMaterialesId]) REFERENCES [produccion].[ListaMateriales] ([Id]),
    CONSTRAINT [FK_OrdenProd_Ruta] FOREIGN KEY ([RutaProduccionId]) REFERENCES [produccion].[RutaProduccion] ([Id]),
    CONSTRAINT [CK_OrdenProd_Estado] CHECK ([Estado] IN ('Planificada', 'Liberada', 'EnProceso', 'Terminada', 'Cerrada', 'Anulada')),
    CONSTRAINT [CK_OrdenProd_CantPlan] CHECK ([CantidadPlanificada] > 0),
    CONSTRAINT [CK_OrdenProd_CantTerm] CHECK ([CantidadTerminada] >= 0 AND [CantidadMerma] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_OrdenProduccion_Empresa_Numero]
    ON [produccion].[OrdenProduccion] ([TenantId], [EmpresaId], [Numero]);
GO

CREATE TABLE [produccion].[MovimientoProduccion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MovimientoProduccion] PRIMARY KEY,
    [OrdenProduccionId] uniqueidentifier NOT NULL,
    [TipoMovimiento] varchar(20) NOT NULL,
    [ComponenteReferenciaId] uniqueidentifier NULL,
    [Cantidad] decimal(28, 10) NOT NULL,
    [CostoUnitario] decimal(28, 10) NULL,
    [FechaMovimiento] date NOT NULL,
    [Notas] nvarchar(500) NULL,
    [OcurridoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_MovProd_Ocurrido] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_MovProd_Orden] FOREIGN KEY ([OrdenProduccionId]) REFERENCES [produccion].[OrdenProduccion] ([Id]),
    CONSTRAINT [CK_MovProd_Tipo] CHECK ([TipoMovimiento] IN ('Consumo', 'Devolucion', 'Terminado', 'Merma')),
    CONSTRAINT [CK_MovProd_Cantidad] CHECK ([Cantidad] > 0)
);
GO

CREATE INDEX [IX_MovimientoProduccion_Orden_Tipo]
    ON [produccion].[MovimientoProduccion] ([OrdenProduccionId], [TipoMovimiento], [FechaMovimiento] DESC);
GO

CREATE TABLE [produccion].[CostoOrdenProduccion]
(
    [OrdenProduccionId] uniqueidentifier NOT NULL CONSTRAINT [PK_CostoOrdenProduccion] PRIMARY KEY,
    [CostoMateriaPrima] decimal(28, 10) NOT NULL CONSTRAINT [DF_CostoOrden_Mp] DEFAULT (0),
    [CostoManoObra] decimal(28, 10) NOT NULL CONSTRAINT [DF_CostoOrden_Mo] DEFAULT (0),
    [CostoIndirecto] decimal(28, 10) NOT NULL CONSTRAINT [DF_CostoOrden_Cif] DEFAULT (0),
    [CostoTotal] decimal(28, 10) NOT NULL CONSTRAINT [DF_CostoOrden_Total] DEFAULT (0),
    [CostoUnitarioTerminado] decimal(28, 10) NOT NULL CONSTRAINT [DF_CostoOrden_Unit] DEFAULT (0),
    [CalculadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_CostoOrden_Calc] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_CostoOrden_Orden] FOREIGN KEY ([OrdenProduccionId]) REFERENCES [produccion].[OrdenProduccion] ([Id])
);
GO
