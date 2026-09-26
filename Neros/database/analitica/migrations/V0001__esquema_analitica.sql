/*
===============================================================================
Neros ERP
Script        : V0001__esquema_analitica.sql
Modulo        : analitica
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Warehouse de lectura: marcas de agua, eventos ingeridos, hechos e indicadores (D-17a).
Dependencias  : Ninguna
Objetos       : analitica.MarcaAguaIngesta, EventoIngesta, HechoOperativo, DefinicionIndicador, ValorIndicador
Motivo        : BI desacoplado del OLTP; proyecciones alimentadas por eventos.
Impacto       : Esquema y tablas nuevas en base dedicada.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_analitica.sql
===============================================================================
*/

CREATE SCHEMA [analitica];
GO

CREATE TABLE [analitica].[MarcaAguaIngesta]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MarcaAguaIngesta] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [FuenteModulo] varchar(40) NOT NULL,
    [UltimoInstanteUtc] datetime2(3) NOT NULL,
    [UltimoEventoId] uniqueidentifier NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_MarcaAgua_Actualizado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_MarcaAgua_Fuente] CHECK ([FuenteModulo] <> '')
);
GO

CREATE UNIQUE INDEX [UX_MarcaAgua_Tenant_Empresa_Fuente]
    ON [analitica].[MarcaAguaIngesta] ([TenantId], [EmpresaId], [FuenteModulo]);
GO

CREATE TABLE [analitica].[EventoIngesta]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_EventoIngesta] PRIMARY KEY,
    [MessageId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [FuenteModulo] varchar(40) NOT NULL,
    [TipoEvento] varchar(120) NOT NULL,
    [AggregateId] varchar(200) NOT NULL,
    [VersionAgregado] bigint NOT NULL,
    [OcurrioEnUtc] datetime2(3) NOT NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [CausationId] uniqueidentifier NULL,
    [EsTardio] bit NOT NULL CONSTRAINT [DF_EventoIngesta_Tardio] DEFAULT (0),
    [PayloadJson] nvarchar(max) NOT NULL,
    [IngestadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_EventoIngesta_Ingestado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_EventoIngesta_Payload] CHECK (ISJSON([PayloadJson]) = 1)
);
GO

CREATE UNIQUE INDEX [UX_EventoIngesta_MessageId]
    ON [analitica].[EventoIngesta] ([MessageId]);
GO

CREATE INDEX [IX_EventoIngesta_Empresa_Ocurrio]
    ON [analitica].[EventoIngesta] ([TenantId], [EmpresaId], [OcurrioEnUtc] DESC);
GO

CREATE TABLE [analitica].[HechoOperativo]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_HechoOperativo] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoHecho] varchar(60) NOT NULL,
    [PeriodoNegocio] date NOT NULL,
    [ImporteMonedaFuncional] decimal(19, 4) NOT NULL,
    [Cantidad] decimal(19, 4) NULL,
    [DimensionClave] varchar(120) NULL,
    [EventoIngestaId] uniqueidentifier NOT NULL,
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_HechoOperativo_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_HechoOperativo_Evento] FOREIGN KEY ([EventoIngestaId]) REFERENCES [analitica].[EventoIngesta] ([Id])
);
GO

CREATE INDEX [IX_HechoOperativo_Periodo_Tipo]
    ON [analitica].[HechoOperativo] ([TenantId], [EmpresaId], [PeriodoNegocio], [TipoHecho]);
GO

CREATE TABLE [analitica].[DefinicionIndicador]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DefinicionIndicador] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(40) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [TipoIndicador] varchar(40) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_DefinicionIndicador_Activo] DEFAULT (1),
    CONSTRAINT [CK_DefinicionIndicador_Tipo] CHECK ([TipoIndicador] IN (
        'VentasNetas', 'ComprasRecibidas', 'SaldoCartera', 'EjecucionPresupuesto', 'NominaNeta'))
);
GO

CREATE UNIQUE INDEX [UX_DefinicionIndicador_Empresa_Codigo]
    ON [analitica].[DefinicionIndicador] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [analitica].[ValorIndicador]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ValorIndicador] PRIMARY KEY,
    [DefinicionIndicadorId] uniqueidentifier NOT NULL,
    [PeriodoNegocio] date NOT NULL,
    [Valor] decimal(19, 4) NOT NULL,
    [CalculadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ValorIndicador_Calculado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_ValorIndicador_Definicion] FOREIGN KEY ([DefinicionIndicadorId]) REFERENCES [analitica].[DefinicionIndicador] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_ValorIndicador_Periodo]
    ON [analitica].[ValorIndicador] ([DefinicionIndicadorId], [PeriodoNegocio]);
GO
