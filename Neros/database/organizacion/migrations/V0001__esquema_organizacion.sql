/*
===============================================================================
Neros ERP
Script        : V0001__esquema_organizacion.sql
Modulo        : organizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Modelo Tenant, grupo empresarial, empresa (correspondencia) y sucursal
                con configuracion regional por empresa.
Dependencias  : Ninguna
Objetos       : organizacion.Tenant, GrupoEmpresarial, Empresa, Sucursal
Motivo        : Plan maestro fase 4 (D-05/D-07).
Impacto       : Base propia del modulo Organization; sin FK a compatibilidad.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_organizacion.sql
===============================================================================
*/

CREATE SCHEMA [organizacion];
GO

CREATE TABLE [organizacion].[Tenant]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Tenant] PRIMARY KEY,
    [Codigo] varchar(40) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Tenant_Activo] DEFAULT (1)
);
GO

CREATE UNIQUE INDEX [UX_Tenant_Codigo] ON [organizacion].[Tenant] ([Codigo]);
GO

CREATE TABLE [organizacion].[GrupoEmpresarial]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_GrupoEmpresarial] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [Codigo] varchar(40) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_GrupoEmpresarial_Activo] DEFAULT (1),
    CONSTRAINT [FK_GrupoEmpresarial_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [organizacion].[Tenant] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_GrupoEmpresarial_Tenant_Codigo]
    ON [organizacion].[GrupoEmpresarial] ([TenantId], [Codigo]);
GO

CREATE TABLE [organizacion].[Empresa]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Empresa] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [GrupoEmpresarialId] uniqueidentifier NULL,
    [Codigo] varchar(20) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Identificacion] nvarchar(30) NOT NULL,
    [Activa] bit NOT NULL CONSTRAINT [DF_Empresa_Activa] DEFAULT (1),
    [Pais] char(2) NOT NULL CONSTRAINT [DF_Empresa_Pais] DEFAULT ('CO'),
    [MonedaFuncional] char(3) NOT NULL CONSTRAINT [DF_Empresa_Moneda] DEFAULT ('COP'),
    [ZonaHoraria] varchar(64) NOT NULL CONSTRAINT [DF_Empresa_Zona] DEFAULT ('America/Bogota'),
    [CulturaFormato] varchar(10) NOT NULL CONSTRAINT [DF_Empresa_Cultura] DEFAULT ('es-CO'),
    [MarcoContable] varchar(20) NOT NULL CONSTRAINT [DF_Empresa_Marco] DEFAULT ('Local'),
    CONSTRAINT [FK_Empresa_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [organizacion].[Tenant] ([Id]),
    CONSTRAINT [FK_Empresa_Grupo] FOREIGN KEY ([GrupoEmpresarialId]) REFERENCES [organizacion].[GrupoEmpresarial] ([Id]),
    CONSTRAINT [CK_Empresa_Marco] CHECK ([MarcoContable] IN ('Local', 'IFRS', 'USGAAP'))
);
GO

CREATE INDEX [IX_Empresa_Tenant] ON [organizacion].[Empresa] ([TenantId], [Codigo]);
GO

CREATE TABLE [organizacion].[Sucursal]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Sucursal] PRIMARY KEY DEFAULT (NEWSEQUENTIALID()),
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(20) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Activa] bit NOT NULL CONSTRAINT [DF_Sucursal_Activa] DEFAULT (1),
    CONSTRAINT [FK_Sucursal_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [organizacion].[Tenant] ([Id]),
    CONSTRAINT [FK_Sucursal_Empresa] FOREIGN KEY ([EmpresaId]) REFERENCES [organizacion].[Empresa] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_Sucursal_Empresa_Codigo]
    ON [organizacion].[Sucursal] ([EmpresaId], [Codigo]);
GO

CREATE INDEX [IX_Sucursal_Tenant] ON [organizacion].[Sucursal] ([TenantId], [EmpresaId]);
GO
