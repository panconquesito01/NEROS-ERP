/*
===============================================================================
Neros ERP
Script        : V0002__catalogos_transversales.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Catalogos transversales a todas las empresas: zonas horarias,
                unidades de medida y tipos de identificacion por pais (§30).
Dependencias  : V0001__esquema_globalizacion.sql
Objetos       : globalizacion.ZonaHoraria, UnidadMedida, TipoIdentificacion
Motivo        : Referencia compartida sin TenantId/EmpresaId.
Impacto       : Tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/002_catalogos_transversales.sql
===============================================================================
*/

CREATE TABLE [globalizacion].[ZonaHoraria]
(
    [Id] varchar(64) NOT NULL CONSTRAINT [PK_ZonaHoraria] PRIMARY KEY,
    [Nombre] nvarchar(160) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_ZonaHoraria_Activo] DEFAULT (1)
);
GO

CREATE TABLE [globalizacion].[UnidadMedida]
(
    [Codigo] varchar(16) NOT NULL CONSTRAINT [PK_UnidadMedida] PRIMARY KEY,
    [Nombre] nvarchar(120) NOT NULL,
    [Simbolo] nvarchar(16) NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_UnidadMedida_Activo] DEFAULT (1)
);
GO

CREATE TABLE [globalizacion].[TipoIdentificacion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_TipoIdentificacion] PRIMARY KEY DEFAULT (NEWID()),
    [Pais] char(2) NOT NULL,
    [Codigo] varchar(20) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_TipoIdentificacion_Activo] DEFAULT (1),
    CONSTRAINT [FK_TipoIdentificacion_Pais] FOREIGN KEY ([Pais]) REFERENCES [globalizacion].[Pais] ([Codigo])
);
GO

CREATE UNIQUE INDEX [UX_TipoIdentificacion_Pais_Codigo]
    ON [globalizacion].[TipoIdentificacion] ([Pais], [Codigo]);
GO
