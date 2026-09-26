/*
===============================================================================
Neros ERP
Script        : V0001__esquema_globalizacion.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Catalogos ISO, politicas de redondeo, tasas de cambio y
                registro normativo (plan fase 6, §12, §29, §31, §32).
Dependencias  : Ninguna
Objetos       : globalizacion.Pais, Moneda, PoliticaRedondeo, TasaCambio,
                PaqueteLocalizacion; cumplimiento.ReglaNormativa
Motivo        : Base de globalizacion y cumplimiento transversal.
Impacto       : Esquemas y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_globalizacion.sql
===============================================================================
*/

CREATE SCHEMA [globalizacion];
GO
CREATE SCHEMA [cumplimiento];
GO

CREATE TABLE [globalizacion].[Pais]
(
    [Codigo] char(2) NOT NULL CONSTRAINT [PK_Pais] PRIMARY KEY,
    [Nombre] nvarchar(120) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Pais_Activo] DEFAULT (1)
);
GO

CREATE TABLE [globalizacion].[Moneda]
(
    [Codigo] char(3) NOT NULL CONSTRAINT [PK_Moneda] PRIMARY KEY,
    [Nombre] nvarchar(120) NOT NULL,
    [DecimalesIso4217] tinyint NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Moneda_Activo] DEFAULT (1)
);
GO

CREATE TABLE [globalizacion].[PoliticaRedondeo]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PoliticaRedondeo] PRIMARY KEY DEFAULT (NEWID()),
    [Moneda] char(3) NOT NULL,
    [TipoDocumento] varchar(40) NOT NULL,
    [PrecisionCalculo] tinyint NOT NULL,
    [PrecisionMoneda] tinyint NOT NULL,
    [ModoRedondeo] varchar(20) NOT NULL CONSTRAINT [CK_PoliticaRedondeo_Modo]
        CHECK ([ModoRedondeo] IN ('AwayFromZero', 'ToEven', 'Truncate')),
    [MomentoRedondeo] varchar(20) NOT NULL CONSTRAINT [CK_PoliticaRedondeo_Momento]
        CHECK ([MomentoRedondeo] IN ('PorLinea', 'PorTotal')),
    [Activo] bit NOT NULL CONSTRAINT [DF_PoliticaRedondeo_Activo] DEFAULT (1),
    CONSTRAINT [FK_PoliticaRedondeo_Moneda] FOREIGN KEY ([Moneda]) REFERENCES [globalizacion].[Moneda] ([Codigo])
);
GO

CREATE UNIQUE INDEX [UX_PoliticaRedondeo_Moneda_TipoDocumento]
    ON [globalizacion].[PoliticaRedondeo] ([Moneda], [TipoDocumento]);
GO

CREATE TABLE [globalizacion].[TasaCambio]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_TasaCambio] PRIMARY KEY DEFAULT (NEWID()),
    [Fuente] varchar(40) NOT NULL,
    [Fecha] date NOT NULL,
    [MonedaOrigen] char(3) NOT NULL,
    [MonedaDestino] char(3) NOT NULL,
    [Valor] decimal(28, 12) NOT NULL CONSTRAINT [CK_TasaCambio_Valor] CHECK ([Valor] > 0),
    CONSTRAINT [FK_TasaCambio_MonedaOrigen] FOREIGN KEY ([MonedaOrigen]) REFERENCES [globalizacion].[Moneda] ([Codigo]),
    CONSTRAINT [FK_TasaCambio_MonedaDestino] FOREIGN KEY ([MonedaDestino]) REFERENCES [globalizacion].[Moneda] ([Codigo])
);
GO

CREATE UNIQUE INDEX [UX_TasaCambio_Fuente_Fecha_Par]
    ON [globalizacion].[TasaCambio] ([Fuente], [Fecha], [MonedaOrigen], [MonedaDestino]);
GO

CREATE TABLE [globalizacion].[PaqueteLocalizacion]
(
    [Codigo] varchar(40) NOT NULL CONSTRAINT [PK_PaqueteLocalizacion] PRIMARY KEY,
    [Pais] char(2) NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Version] varchar(20) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_PaqueteLocalizacion_Activo] DEFAULT (1),
    CONSTRAINT [FK_PaqueteLocalizacion_Pais] FOREIGN KEY ([Pais]) REFERENCES [globalizacion].[Pais] ([Codigo])
);
GO

CREATE TABLE [cumplimiento].[ReglaNormativa]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ReglaNormativa] PRIMARY KEY DEFAULT (NEWID()),
    [Pais] char(2) NOT NULL,
    [Jurisdiccion] nvarchar(80) NOT NULL,
    [Modulo] varchar(40) NOT NULL,
    [Codigo] varchar(40) NOT NULL,
    [Nombre] nvarchar(200) NOT NULL,
    [Descripcion] nvarchar(1000) NOT NULL,
    [Fuente] nvarchar(500) NOT NULL,
    [VigenteDesde] date NULL,
    [VigenteHasta] date NULL,
    [Version] varchar(20) NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [CK_ReglaNormativa_Estado]
        CHECK ([Estado] IN ('POR_VERIFICAR', 'VERIFICADA', 'DEROGADA')),
    [RevisadoEnUtc] datetime2(3) NULL,
    [RevisadoPor] nvarchar(120) NULL,
    CONSTRAINT [FK_ReglaNormativa_Pais] FOREIGN KEY ([Pais]) REFERENCES [globalizacion].[Pais] ([Codigo])
);
GO

CREATE UNIQUE INDEX [UX_ReglaNormativa_Pais_Modulo_Codigo_Version]
    ON [cumplimiento].[ReglaNormativa] ([Pais], [Modulo], [Codigo], [Version]);
GO
