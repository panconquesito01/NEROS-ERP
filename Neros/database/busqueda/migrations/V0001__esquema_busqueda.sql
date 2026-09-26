/*
===============================================================================
Neros ERP
Script        : V0001__esquema_busqueda.sql
Modulo        : busqueda
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Indice de busqueda desacoplado: ACL, tombstones, version y frescura (D-17b).
Dependencias  : Ninguna
Objetos       : busqueda.DocumentoIndice, EventoIndexacion, CheckpointIngesta, EnlaceVista360
Motivo        : Busqueda global sin joins distribuidos al OLTP.
Impacto       : Esquema y tablas nuevas en base dedicada.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_busqueda.sql
===============================================================================
*/

CREATE SCHEMA [busqueda];
GO

CREATE TABLE [busqueda].[CheckpointIngesta]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_CheckpointIngesta] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [FuenteModulo] varchar(40) NOT NULL,
    [UltimoInstanteUtc] datetime2(3) NOT NULL,
    [UltimoEventoId] uniqueidentifier NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Checkpoint_Actualizado] DEFAULT (sysutcdatetime())
);
GO

CREATE UNIQUE INDEX [UX_Checkpoint_Tenant_Empresa_Fuente]
    ON [busqueda].[CheckpointIngesta] ([TenantId], [EmpresaId], [FuenteModulo]);
GO

CREATE TABLE [busqueda].[EventoIndexacion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_EventoIndexacion] PRIMARY KEY,
    [MessageId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [FuenteModulo] varchar(40) NOT NULL,
    [TipoEvento] varchar(120) NOT NULL,
    [EntidadId] varchar(200) NOT NULL,
    [VersionAgregado] bigint NOT NULL,
    [OcurrioEnUtc] datetime2(3) NOT NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [EsTombstone] bit NOT NULL CONSTRAINT [DF_EventoIndexacion_Tombstone] DEFAULT (0),
    [IndexadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_EventoIndexacion_Indexado] DEFAULT (sysutcdatetime())
);
GO

CREATE UNIQUE INDEX [UX_EventoIndexacion_MessageId]
    ON [busqueda].[EventoIndexacion] ([MessageId]);
GO

CREATE TABLE [busqueda].[DocumentoIndice]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoIndice] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoEntidad] varchar(60) NOT NULL,
    [EntidadId] varchar(200) NOT NULL,
    [VersionIndice] bigint NOT NULL,
    [Titulo] nvarchar(200) NOT NULL,
    [Resumen] nvarchar(500) NULL,
    [TextoBusqueda] nvarchar(1000) NOT NULL,
    [PermisoRequerido] varchar(80) NOT NULL,
    [OrigenModulo] varchar(40) NOT NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_DocumentoIndice_Activo] DEFAULT (1),
    [TombstoneEnUtc] datetime2(3) NULL,
    [IndexadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_DocumentoIndice_Indexado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_DocumentoIndice_Version] CHECK ([VersionIndice] > 0)
);
GO

CREATE UNIQUE INDEX [UX_DocumentoIndice_Entidad]
    ON [busqueda].[DocumentoIndice] ([TenantId], [EmpresaId], [TipoEntidad], [EntidadId]);
GO

CREATE INDEX [IX_DocumentoIndice_Busqueda]
    ON [busqueda].[DocumentoIndice] ([TenantId], [EmpresaId], [Activo], [TombstoneEnUtc])
    INCLUDE ([Titulo], [TextoBusqueda], [PermisoRequerido], [IndexadoEnUtc]);
GO

CREATE TABLE [busqueda].[EnlaceVista360]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_EnlaceVista360] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [DocumentoOrigenId] uniqueidentifier NOT NULL,
    [DocumentoRelacionadoId] uniqueidentifier NOT NULL,
    [TipoRelacion] varchar(40) NOT NULL,
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_EnlaceVista360_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_EnlaceVista360_Origen] FOREIGN KEY ([DocumentoOrigenId]) REFERENCES [busqueda].[DocumentoIndice] ([Id]),
    CONSTRAINT [FK_EnlaceVista360_Relacionado] FOREIGN KEY ([DocumentoRelacionadoId]) REFERENCES [busqueda].[DocumentoIndice] ([Id]),
    CONSTRAINT [CK_EnlaceVista360_Distinto] CHECK ([DocumentoOrigenId] <> [DocumentoRelacionadoId])
);
GO

CREATE UNIQUE INDEX [UX_EnlaceVista360_Par]
    ON [busqueda].[EnlaceVista360] ([DocumentoOrigenId], [DocumentoRelacionadoId], [TipoRelacion]);
GO
