/*
===============================================================================
Neros ERP
Script        : V0001__esquema_activos_fijos.sql
Modulo        : activos
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Categorias, activos fijos, depreciacion lineal, deterioro y baja (plan fase 20, §54).
Dependencias  : Ninguna
Objetos       : activos.CategoriaActivo, ActivoFijo, DepreciacionPeriodo, EventoActivo
Motivo        : Base de activos fijos.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_activos.sql
===============================================================================
*/

CREATE SCHEMA [activos];
GO

CREATE TABLE [activos].[CategoriaActivo]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_CategoriaActivo] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [VidaUtilMesesDefault] int NOT NULL,
    [MetodoDepreciacionDefault] varchar(20) NOT NULL CONSTRAINT [DF_Categoria_Metodo] DEFAULT ('Lineal'),
    [Activo] bit NOT NULL CONSTRAINT [DF_Categoria_Activo] DEFAULT (1),
    CONSTRAINT [CK_Categoria_VidaUtil] CHECK ([VidaUtilMesesDefault] > 0),
    CONSTRAINT [CK_Categoria_Metodo] CHECK ([MetodoDepreciacionDefault] IN ('Lineal'))
);
GO

CREATE UNIQUE INDEX [UX_CategoriaActivo_Empresa_Codigo]
    ON [activos].[CategoriaActivo] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [activos].[ActivoFijo]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ActivoFijo] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [CategoriaActivoId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Descripcion] nvarchar(240) NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_ActivoFijo_Estado] DEFAULT ('Borrador'),
    [FechaAdquisicion] date NOT NULL,
    [CostoAdquisicion] decimal(28, 10) NOT NULL,
    [ValorResidual] decimal(28, 10) NOT NULL CONSTRAINT [DF_ActivoFijo_Residual] DEFAULT (0),
    [VidaUtilMeses] int NOT NULL,
    [MetodoDepreciacion] varchar(20) NOT NULL CONSTRAINT [DF_ActivoFijo_Metodo] DEFAULT ('Lineal'),
    [DepreciacionAcumulada] decimal(28, 10) NOT NULL CONSTRAINT [DF_ActivoFijo_DepAcum] DEFAULT (0),
    [DeterioroAcumulado] decimal(28, 10) NOT NULL CONSTRAINT [DF_ActivoFijo_DetAcum] DEFAULT (0),
    [ValorEnLibros] decimal(28, 10) NOT NULL CONSTRAINT [DF_ActivoFijo_ValorLibros] DEFAULT (0),
    [FechaBaja] date NULL,
    [MotivoBaja] nvarchar(500) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ActivoFijo_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_ActivoFijo_Categoria] FOREIGN KEY ([CategoriaActivoId]) REFERENCES [activos].[CategoriaActivo] ([Id]),
    CONSTRAINT [CK_ActivoFijo_Estado] CHECK ([Estado] IN ('Borrador', 'Activo', 'DadoDeBaja')),
    CONSTRAINT [CK_ActivoFijo_Costo] CHECK ([CostoAdquisicion] >= 0),
    CONSTRAINT [CK_ActivoFijo_Residual] CHECK ([ValorResidual] >= 0 AND [ValorResidual] <= [CostoAdquisicion]),
    CONSTRAINT [CK_ActivoFijo_VidaUtil] CHECK ([VidaUtilMeses] > 0),
    CONSTRAINT [CK_ActivoFijo_Metodo] CHECK ([MetodoDepreciacion] IN ('Lineal'))
);
GO

CREATE UNIQUE INDEX [UX_ActivoFijo_Empresa_Codigo]
    ON [activos].[ActivoFijo] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [activos].[DepreciacionPeriodo]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DepreciacionPeriodo] PRIMARY KEY,
    [ActivoFijoId] uniqueidentifier NOT NULL,
    [Anio] int NOT NULL,
    [Mes] int NOT NULL,
    [ImporteDepreciacion] decimal(28, 10) NOT NULL,
    [DepreciacionAcumulada] decimal(28, 10) NOT NULL,
    [ValorEnLibros] decimal(28, 10) NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_DepPeriodo_Estado] DEFAULT ('Calculada'),
    [ContabilizadaEnUtc] datetime2(3) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_DepPeriodo_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_DepreciacionPeriodo_Activo] FOREIGN KEY ([ActivoFijoId]) REFERENCES [activos].[ActivoFijo] ([Id]),
    CONSTRAINT [CK_DepPeriodo_Mes] CHECK ([Mes] BETWEEN 1 AND 12),
    CONSTRAINT [CK_DepPeriodo_Importe] CHECK ([ImporteDepreciacion] >= 0),
    CONSTRAINT [CK_DepPeriodo_Estado] CHECK ([Estado] IN ('Calculada', 'Contabilizada'))
);
GO

CREATE UNIQUE INDEX [UX_DepreciacionPeriodo_Activo_AnioMes]
    ON [activos].[DepreciacionPeriodo] ([ActivoFijoId], [Anio], [Mes]);
GO

CREATE TABLE [activos].[EventoActivo]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_EventoActivo] PRIMARY KEY,
    [ActivoFijoId] uniqueidentifier NOT NULL,
    [TipoEvento] varchar(30) NOT NULL,
    [FechaEvento] date NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [Notas] nvarchar(500) NULL,
    [OcurridoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_EventoActivo_Ocurrido] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_EventoActivo_Activo] FOREIGN KEY ([ActivoFijoId]) REFERENCES [activos].[ActivoFijo] ([Id]),
    CONSTRAINT [CK_EventoActivo_Tipo] CHECK ([TipoEvento] IN ('Adquisicion', 'Depreciacion', 'Deterioro', 'Baja'))
);
GO

CREATE INDEX [IX_EventoActivo_Activo_Fecha]
    ON [activos].[EventoActivo] ([ActivoFijoId], [FechaEvento] DESC);
GO
