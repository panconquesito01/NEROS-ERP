/*
===============================================================================
Neros ERP
Script        : V0001__esquema_privacidad.sql
Modulo        : privacidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Infraestructura minima de privacidad: inventario de datos,
                documentos legales versionados, aceptaciones, cookies y
                retencion con legal hold.
Dependencias  : Ninguna
Objetos       : privacidad.CatalogoDatoPersonal, DocumentoLegal,
                DocumentoLegalVersion, AceptacionLegal, DefinicionCookie,
                RetencionPolicy, LegalHold
Motivo        : Plan maestro fase 3 (§22-§27).
Impacto       : Esquema y tablas nuevas; sin datos de clientes.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_privacidad.sql
===============================================================================
*/

CREATE SCHEMA [privacidad];
GO

CREATE TABLE [privacidad].[RetencionPolicy]
(
    [Codigo] varchar(40) NOT NULL CONSTRAINT [PK_RetencionPolicy] PRIMARY KEY,
    [TipoInformacion] nvarchar(120) NOT NULL,
    [Jurisdiccion] varchar(10) NOT NULL,
    [PeriodoDias] int NULL,
    [PeriodoDescripcion] nvarchar(200) NULL,
    [InicioComputo] nvarchar(120) NOT NULL,
    [Fundamento] nvarchar(500) NOT NULL,
    [AccionFinal] varchar(20) NOT NULL CONSTRAINT [CK_RetencionPolicy_AccionFinal]
        CHECK ([AccionFinal] IN ('Conservar', 'Archivar', 'Anonimizar', 'Eliminar', 'Bloquear')),
    [Activo] bit NOT NULL CONSTRAINT [DF_RetencionPolicy_Activo] DEFAULT (1)
);
GO

CREATE TABLE [privacidad].[CatalogoDatoPersonal]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_CatalogoDatoPersonal] PRIMARY KEY DEFAULT (NEWID()),
    [Campo] varchar(120) NOT NULL,
    [Modulo] varchar(40) NOT NULL,
    [Clasificacion] varchar(20) NOT NULL CONSTRAINT [CK_CatalogoDatoPersonal_Clasificacion]
        CHECK ([Clasificacion] IN ('Personal', 'Restringido', 'Sensible')),
    [Finalidad] nvarchar(500) NOT NULL,
    [Origen] nvarchar(200) NOT NULL,
    [RetencionCodigo] varchar(40) NOT NULL,
    [PermisoRequerido] varchar(80) NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_CatalogoDatoPersonal_Activo] DEFAULT (1),
    CONSTRAINT [FK_CatalogoDatoPersonal_RetencionPolicy] FOREIGN KEY ([RetencionCodigo])
        REFERENCES [privacidad].[RetencionPolicy] ([Codigo])
);
GO

CREATE UNIQUE INDEX [UX_CatalogoDatoPersonal_Campo_Modulo]
    ON [privacidad].[CatalogoDatoPersonal] ([Campo], [Modulo]);
GO

CREATE TABLE [privacidad].[DocumentoLegal]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoLegal] PRIMARY KEY DEFAULT (NEWID()),
    [Codigo] varchar(40) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [RequiereAceptacion] bit NOT NULL CONSTRAINT [DF_DocumentoLegal_RequiereAceptacion] DEFAULT (1),
    [Orden] int NOT NULL CONSTRAINT [DF_DocumentoLegal_Orden] DEFAULT (0),
    [Activo] bit NOT NULL CONSTRAINT [DF_DocumentoLegal_Activo] DEFAULT (1)
);
GO

CREATE UNIQUE INDEX [UX_DocumentoLegal_Codigo] ON [privacidad].[DocumentoLegal] ([Codigo]);
GO

CREATE TABLE [privacidad].[DocumentoLegalVersion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoLegalVersion] PRIMARY KEY DEFAULT (NEWID()),
    [DocumentoId] uniqueidentifier NOT NULL,
    [Version] int NOT NULL,
    [Contenido] nvarchar(max) NOT NULL,
    [HashContenido] char(64) NOT NULL,
    [VigenteDesde] datetime2(3) NOT NULL,
    [VigenteHasta] datetime2(3) NULL,
    [PublicadoPor] nvarchar(128) NULL,
    CONSTRAINT [FK_DocumentoLegalVersion_Documento] FOREIGN KEY ([DocumentoId])
        REFERENCES [privacidad].[DocumentoLegal] ([Id]),
    CONSTRAINT [CK_DocumentoLegalVersion_Hash] CHECK ([HashContenido] NOT LIKE '%[^0-9A-Fa-f]%')
);
GO

CREATE UNIQUE INDEX [UX_DocumentoLegalVersion_Documento_Version]
    ON [privacidad].[DocumentoLegalVersion] ([DocumentoId], [Version]);
GO

CREATE INDEX [IX_DocumentoLegalVersion_Vigencia]
    ON [privacidad].[DocumentoLegalVersion] ([DocumentoId], [VigenteDesde], [VigenteHasta]);
GO

CREATE TABLE [privacidad].[AceptacionLegal]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AceptacionLegal] PRIMARY KEY DEFAULT (NEWID()),
    [DocumentoId] uniqueidentifier NOT NULL,
    [VersionId] uniqueidentifier NOT NULL,
    [UsuarioId] nvarchar(450) NOT NULL,
    [FechaUtc] datetime2(3) NOT NULL CONSTRAINT [DF_AceptacionLegal_FechaUtc] DEFAULT (SYSUTCDATETIME()),
    [Ip] nvarchar(45) NULL,
    [AgenteUsuario] nvarchar(256) NULL,
    [HashContenido] char(64) NOT NULL,
    [FormaAceptacion] varchar(40) NOT NULL CONSTRAINT [CK_AceptacionLegal_Forma]
        CHECK ([FormaAceptacion] IN ('Explicita', 'ConsentimientoCookies', 'Registro')),
    CONSTRAINT [FK_AceptacionLegal_Documento] FOREIGN KEY ([DocumentoId])
        REFERENCES [privacidad].[DocumentoLegal] ([Id]),
    CONSTRAINT [FK_AceptacionLegal_Version] FOREIGN KEY ([VersionId])
        REFERENCES [privacidad].[DocumentoLegalVersion] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_AceptacionLegal_Usuario_Version]
    ON [privacidad].[AceptacionLegal] ([UsuarioId], [VersionId]);
GO

CREATE INDEX [IX_AceptacionLegal_Usuario_Fecha]
    ON [privacidad].[AceptacionLegal] ([UsuarioId], [FechaUtc] DESC);
GO

CREATE TABLE [privacidad].[DefinicionCookie]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DefinicionCookie] PRIMARY KEY DEFAULT (NEWID()),
    [Nombre] nvarchar(120) NOT NULL,
    [Almacenamiento] varchar(20) NOT NULL CONSTRAINT [CK_DefinicionCookie_Almacenamiento]
        CHECK ([Almacenamiento] IN ('Cookie', 'LocalStorage')),
    [Proveedor] nvarchar(80) NOT NULL,
    [Categoria] varchar(20) NOT NULL CONSTRAINT [CK_DefinicionCookie_Categoria]
        CHECK ([Categoria] IN ('Esencial', 'Preferencia', 'Analitica', 'Marketing')),
    [Finalidad] nvarchar(500) NOT NULL,
    [Duracion] nvarchar(120) NOT NULL,
    [PrimeraParte] bit NOT NULL CONSTRAINT [DF_DefinicionCookie_PrimeraParte] DEFAULT (1),
    [Dominio] nvarchar(120) NULL,
    [Esencial] bit NOT NULL CONSTRAINT [DF_DefinicionCookie_Esencial] DEFAULT (0),
    [UrlPolitica] nvarchar(500) NULL,
    [Orden] int NOT NULL CONSTRAINT [DF_DefinicionCookie_Orden] DEFAULT (0),
    [Activo] bit NOT NULL CONSTRAINT [DF_DefinicionCookie_Activo] DEFAULT (1)
);
GO

CREATE UNIQUE INDEX [UX_DefinicionCookie_Nombre_Almacenamiento]
    ON [privacidad].[DefinicionCookie] ([Nombre], [Almacenamiento]);
GO

CREATE TABLE [privacidad].[LegalHold]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_LegalHold] PRIMARY KEY DEFAULT (NEWID()),
    [Referencia] nvarchar(120) NOT NULL,
    [Alcance] nvarchar(500) NOT NULL,
    [Motivo] nvarchar(500) NOT NULL,
    [FechaInicioUtc] datetime2(3) NOT NULL,
    [FechaFinUtc] datetime2(3) NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_LegalHold_Activo] DEFAULT (1)
);
GO

CREATE INDEX [IX_LegalHold_Activo] ON [privacidad].[LegalHold] ([Activo], [FechaInicioUtc]);
GO
