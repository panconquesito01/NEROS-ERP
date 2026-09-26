/*
===============================================================================
Neros ERP
Script        : V0001__esquema_presupuesto.sql
Modulo        : presupuesto
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Version de presupuesto, centros de costo y valores por cuenta y periodo (plan fase 21, §54).
Dependencias  : Ninguna
Objetos       : presupuesto.CentroCosto, VersionPresupuesto, LineaPresupuesto
Motivo        : Base de control presupuestal.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_presupuesto.sql
===============================================================================
*/

CREATE SCHEMA [presupuesto];
GO

CREATE TABLE [presupuesto].[CentroCosto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_CentroCosto] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_CentroCosto_Activo] DEFAULT (1),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_CentroCosto_Creado] DEFAULT (sysutcdatetime())
);
GO

CREATE UNIQUE INDEX [UX_CentroCosto_Empresa_Codigo]
    ON [presupuesto].[CentroCosto] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [presupuesto].[VersionPresupuesto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_VersionPresupuesto] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [Anio] int NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_VersionPresupuesto_Estado] DEFAULT ('Borrador'),
    [VigenciaDesde] date NOT NULL,
    [VigenciaHasta] date NOT NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_VersionPresupuesto_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_VersionPresupuesto_Vigencia] CHECK ([VigenciaHasta] >= [VigenciaDesde]),
    CONSTRAINT [CK_VersionPresupuesto_Estado] CHECK ([Estado] IN ('Borrador', 'Aprobada', 'Cerrada'))
);
GO

CREATE UNIQUE INDEX [UX_VersionPresupuesto_Empresa_Codigo]
    ON [presupuesto].[VersionPresupuesto] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [presupuesto].[LineaPresupuesto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_LineaPresupuesto] PRIMARY KEY,
    [VersionPresupuestoId] uniqueidentifier NOT NULL,
    [CentroCostoId] uniqueidentifier NOT NULL,
    [CuentaContableId] uniqueidentifier NOT NULL,
    [Anio] int NOT NULL,
    [Mes] int NOT NULL,
    [ValorPresupuestado] decimal(28, 10) NOT NULL CONSTRAINT [DF_LineaPresup_Valor] DEFAULT (0),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_LineaPresup_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_LineaPresupuesto_Version] FOREIGN KEY ([VersionPresupuestoId]) REFERENCES [presupuesto].[VersionPresupuesto] ([Id]),
    CONSTRAINT [FK_LineaPresupuesto_CentroCosto] FOREIGN KEY ([CentroCostoId]) REFERENCES [presupuesto].[CentroCosto] ([Id]),
    CONSTRAINT [CK_LineaPresup_Mes] CHECK ([Mes] BETWEEN 1 AND 12),
    CONSTRAINT [CK_LineaPresup_Valor] CHECK ([ValorPresupuestado] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_LineaPresupuesto_ClaveNatural]
    ON [presupuesto].[LineaPresupuesto] ([VersionPresupuestoId], [CentroCostoId], [CuentaContableId], [Anio], [Mes]);
GO

CREATE TABLE [presupuesto].[EjecucionPresupuesto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_EjecucionPresupuesto] PRIMARY KEY,
    [VersionPresupuestoId] uniqueidentifier NOT NULL,
    [CentroCostoId] uniqueidentifier NOT NULL,
    [CuentaContableId] uniqueidentifier NOT NULL,
    [Anio] int NOT NULL,
    [Mes] int NOT NULL,
    [ValorReal] decimal(28, 10) NOT NULL CONSTRAINT [DF_Ejecucion_Real] DEFAULT (0),
    [CalculadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Ejecucion_Calc] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_EjecucionPresupuesto_Version] FOREIGN KEY ([VersionPresupuestoId]) REFERENCES [presupuesto].[VersionPresupuesto] ([Id]),
    CONSTRAINT [FK_EjecucionPresupuesto_CentroCosto] FOREIGN KEY ([CentroCostoId]) REFERENCES [presupuesto].[CentroCosto] ([Id]),
    CONSTRAINT [CK_Ejecucion_Mes] CHECK ([Mes] BETWEEN 1 AND 12)
);
GO

CREATE UNIQUE INDEX [UX_EjecucionPresupuesto_ClaveNatural]
    ON [presupuesto].[EjecucionPresupuesto] ([VersionPresupuestoId], [CentroCostoId], [CuentaContableId], [Anio], [Mes]);
GO
