/*
===============================================================================
Neros ERP
Script        : V0001__esquema_impuestos.sql
Modulo        : impuestos
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Configuracion tributaria y versiones publicadas inmutables
                (plan fase 8, §38).
Dependencias  : Ninguna
Objetos       : impuestos.Jurisdiccion, Impuesto, TarifaImpuesto, ConceptoTributario,
                VersionPublicada, VersionRegla
Motivo        : Base del modulo Impuestos.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_impuestos.sql
===============================================================================
*/

CREATE SCHEMA [impuestos];
GO

CREATE TABLE [impuestos].[Jurisdiccion]
(
    [Codigo] varchar(10) NOT NULL CONSTRAINT [PK_Jurisdiccion] PRIMARY KEY,
    [Nombre] nvarchar(120) NOT NULL,
    [Pais] char(2) NOT NULL
);
GO

CREATE TABLE [impuestos].[Impuesto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Impuesto] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [Codigo] varchar(20) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [JurisdiccionCodigo] varchar(10) NOT NULL,
    [Tipo] varchar(20) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Impuesto_Activo] DEFAULT (1),
    CONSTRAINT [FK_Impuesto_Jurisdiccion] FOREIGN KEY ([JurisdiccionCodigo]) REFERENCES [impuestos].[Jurisdiccion] ([Codigo]),
    CONSTRAINT [CK_Impuesto_Tipo] CHECK ([Tipo] IN ('IVA', 'Retencion', 'Autorretencion', 'Otro'))
);
GO

CREATE UNIQUE INDEX [UX_Impuesto_Tenant_Codigo] ON [impuestos].[Impuesto] ([TenantId], [Codigo]);
GO

CREATE TABLE [impuestos].[TarifaImpuesto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_TarifaImpuesto] PRIMARY KEY DEFAULT (NEWSEQUENTIALID()),
    [ImpuestoId] uniqueidentifier NOT NULL,
    [VigenciaDesde] date NOT NULL,
    [VigenciaHasta] date NULL,
    [Tarifa] decimal(9, 6) NOT NULL,
    [PrecioIncluyeImpuesto] bit NOT NULL CONSTRAINT [DF_Tarifa_Incluido] DEFAULT (0),
    CONSTRAINT [FK_Tarifa_Impuesto] FOREIGN KEY ([ImpuestoId]) REFERENCES [impuestos].[Impuesto] ([Id]),
    CONSTRAINT [CK_Tarifa_Rango] CHECK ([VigenciaHasta] IS NULL OR [VigenciaHasta] >= [VigenciaDesde])
);
GO

CREATE INDEX [IX_Tarifa_Impuesto_Vigencia] ON [impuestos].[TarifaImpuesto] ([ImpuestoId], [VigenciaDesde]);
GO

CREATE TABLE [impuestos].[ConceptoTributario]
(
    [Codigo] varchar(20) NOT NULL CONSTRAINT [PK_ConceptoTributario] PRIMARY KEY,
    [Nombre] nvarchar(160) NOT NULL,
    [JurisdiccionCodigo] varchar(10) NOT NULL,
    CONSTRAINT [FK_Concepto_Jurisdiccion] FOREIGN KEY ([JurisdiccionCodigo]) REFERENCES [impuestos].[Jurisdiccion] ([Codigo])
);
GO

CREATE TABLE [impuestos].[VersionPublicada]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_VersionPublicada] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [Numero] int NOT NULL,
    [PublicadaUtc] datetime2(7) NOT NULL,
    [HashContenido] char(64) NOT NULL,
    CONSTRAINT [CK_VersionPublicada_Numero] CHECK ([Numero] > 0)
);
GO

CREATE UNIQUE INDEX [UX_VersionPublicada_Tenant_Numero]
    ON [impuestos].[VersionPublicada] ([TenantId], [Numero]);
GO

CREATE TABLE [impuestos].[VersionRegla]
(
    [VersionId] uniqueidentifier NOT NULL,
    [Orden] int NOT NULL,
    [ImpuestoCodigo] varchar(20) NOT NULL,
    [Tarifa] decimal(9, 6) NOT NULL,
    [PrecioIncluyeImpuesto] bit NOT NULL,
    [EsRetencion] bit NOT NULL,
    [BaseAcumulaImpuestosPrevios] bit NOT NULL CONSTRAINT [DF_VersionRegla_Acumula] DEFAULT (0),
    CONSTRAINT [PK_VersionRegla] PRIMARY KEY ([VersionId], [Orden]),
    CONSTRAINT [FK_VersionRegla_Version] FOREIGN KEY ([VersionId]) REFERENCES [impuestos].[VersionPublicada] ([Id])
);
GO
