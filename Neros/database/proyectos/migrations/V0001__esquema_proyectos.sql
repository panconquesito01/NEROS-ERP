/*
===============================================================================
Neros ERP
Script        : V0001__esquema_proyectos.sql
Modulo        : proyectos
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Proyecto, fases y presupuesto por proyecto (plan fase 23, §54).
Dependencias  : Ninguna
Objetos       : proyectos.Proyecto, ProyectoFase, PresupuestoProyecto, ReferenciaMovimientoProyecto
Motivo        : ProyectoId transversal en ingresos, gastos, compras, horas y presupuesto.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_proyectos.sql
===============================================================================
*/

CREATE SCHEMA [proyectos];
GO

CREATE TABLE [proyectos].[Proyecto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Proyecto] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Nombre] nvarchar(200) NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Proyecto_Estado] DEFAULT ('Planificado'),
    [ClienteTerceroId] uniqueidentifier NULL,
    [ResponsableTerceroId] uniqueidentifier NULL,
    [FechaInicioPlan] date NOT NULL,
    [FechaFinPlan] date NULL,
    [MonedaCodigo] char(3) NOT NULL CONSTRAINT [DF_Proyecto_Moneda] DEFAULT ('COP'),
    [PresupuestoTotal] decimal(28, 10) NOT NULL CONSTRAINT [DF_Proyecto_Presupuesto] DEFAULT (0),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Proyecto_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_Proyecto_Estado] CHECK ([Estado] IN ('Planificado', 'Activo', 'Suspendido', 'Cerrado', 'Cancelado')),
    CONSTRAINT [CK_Proyecto_Vigencia] CHECK ([FechaFinPlan] IS NULL OR [FechaFinPlan] >= [FechaInicioPlan]),
    CONSTRAINT [CK_Proyecto_Presupuesto] CHECK ([PresupuestoTotal] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_Proyecto_Empresa_Codigo]
    ON [proyectos].[Proyecto] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [proyectos].[ProyectoFase]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ProyectoFase] PRIMARY KEY,
    [ProyectoId] uniqueidentifier NOT NULL,
    [Secuencia] int NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [FechaInicioPlan] date NULL,
    [FechaFinPlan] date NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_ProyectoFase_Estado] DEFAULT ('Pendiente'),
    CONSTRAINT [FK_ProyectoFase_Proyecto] FOREIGN KEY ([ProyectoId]) REFERENCES [proyectos].[Proyecto] ([Id]),
    CONSTRAINT [CK_ProyectoFase_Estado] CHECK ([Estado] IN ('Pendiente', 'EnCurso', 'Completada', 'Cancelada'))
);
GO

CREATE UNIQUE INDEX [UX_ProyectoFase_Secuencia]
    ON [proyectos].[ProyectoFase] ([ProyectoId], [Secuencia]);
GO

CREATE TABLE [proyectos].[PresupuestoProyecto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PresupuestoProyecto] PRIMARY KEY,
    [ProyectoId] uniqueidentifier NOT NULL,
    [Concepto] varchar(30) NOT NULL,
    [ImportePresupuestado] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_PresupuestoProyecto_Proyecto] FOREIGN KEY ([ProyectoId]) REFERENCES [proyectos].[Proyecto] ([Id]),
    CONSTRAINT [CK_PresupuestoProyecto_Concepto] CHECK ([Concepto] IN ('Ingresos', 'Gastos', 'Compras', 'ManoObra', 'Otros')),
    CONSTRAINT [CK_PresupuestoProyecto_Importe] CHECK ([ImportePresupuestado] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_PresupuestoProyecto_Proyecto_Concepto]
    ON [proyectos].[PresupuestoProyecto] ([ProyectoId], [Concepto]);
GO

CREATE TABLE [proyectos].[ReferenciaMovimientoProyecto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ReferenciaMovimientoProyecto] PRIMARY KEY,
    [ProyectoId] uniqueidentifier NOT NULL,
    [ModuloOrigen] varchar(30) NOT NULL,
    [TipoDocumentoOrigen] varchar(40) NOT NULL,
    [DocumentoOrigenId] uniqueidentifier NOT NULL,
    [ImporteAsignado] decimal(28, 10) NOT NULL,
    [FechaNegocio] date NOT NULL,
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_RefMovProy_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_RefMovProyecto_Proyecto] FOREIGN KEY ([ProyectoId]) REFERENCES [proyectos].[Proyecto] ([Id]),
    CONSTRAINT [CK_RefMovProyecto_Modulo] CHECK ([ModuloOrigen] IN ('ventas', 'compras', 'contabilidad', 'nomina', 'produccion', 'presupuesto', 'tesoreria')),
    CONSTRAINT [CK_RefMovProyecto_Importe] CHECK ([ImporteAsignado] >= 0)
);
GO

CREATE INDEX [IX_ReferenciaMovimientoProyecto_Proyecto]
    ON [proyectos].[ReferenciaMovimientoProyecto] ([ProyectoId], [FechaNegocio] DESC);
GO

CREATE UNIQUE INDEX [UX_ReferenciaMovimientoProyecto_Origen]
    ON [proyectos].[ReferenciaMovimientoProyecto] ([ModuloOrigen], [TipoDocumentoOrigen], [DocumentoOrigenId]);
GO
