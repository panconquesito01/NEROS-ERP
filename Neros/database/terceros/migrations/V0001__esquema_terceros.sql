/*
===============================================================================
Neros ERP
Script        : V0001__esquema_terceros.sql
Modulo        : terceros
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Maestro de terceros (identidad, roles, identificacion) con
                historial temporal en la entidad principal (plan fase 7, §36).
Dependencias  : Ninguna
Objetos       : terceros.Tercero, Identificacion, Rol, CuentaBancariaProveedor
Motivo        : Base del modulo Master Data / Terceros.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_terceros.sql
===============================================================================
*/

CREATE SCHEMA [terceros];
GO

CREATE TABLE [terceros].[Tercero]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Tercero] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [Tipo] varchar(20) NOT NULL,
    [RazonSocial] nvarchar(200) NOT NULL,
    [NombreComercial] nvarchar(160) NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Tercero_Activo] DEFAULT (1),
    [ValidFrom] datetime2(7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2(7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [CK_Tercero_Tipo] CHECK ([Tipo] IN ('Persona', 'Organizacion'))
);
GO

ALTER TABLE [terceros].[Tercero]
    SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [terceros].[TerceroHistorial]));
GO

CREATE INDEX [IX_Tercero_Tenant_RazonSocial] ON [terceros].[Tercero] ([TenantId], [RazonSocial]);
GO

CREATE TABLE [terceros].[Identificacion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Identificacion] PRIMARY KEY DEFAULT (NEWSEQUENTIALID()),
    [TenantId] uniqueidentifier NOT NULL,
    [TerceroId] uniqueidentifier NOT NULL,
    [Pais] char(2) NOT NULL,
    [Tipo] varchar(20) NOT NULL,
    [Numero] varchar(30) NOT NULL,
    [DigitoVerificacion] char(1) NULL,
    [EsPrincipal] bit NOT NULL CONSTRAINT [DF_Identificacion_Principal] DEFAULT (0),
    CONSTRAINT [FK_Identificacion_Tercero] FOREIGN KEY ([TerceroId]) REFERENCES [terceros].[Tercero] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_Identificacion_Tenant_Clave]
    ON [terceros].[Identificacion] ([TenantId], [Pais], [Tipo], [Numero]);
GO

CREATE INDEX [IX_Identificacion_Tercero] ON [terceros].[Identificacion] ([TerceroId]);
GO

CREATE TABLE [terceros].[Rol]
(
    [TerceroId] uniqueidentifier NOT NULL,
    [Rol] varchar(20) NOT NULL,
    CONSTRAINT [PK_Rol] PRIMARY KEY ([TerceroId], [Rol]),
    CONSTRAINT [FK_Rol_Tercero] FOREIGN KEY ([TerceroId]) REFERENCES [terceros].[Tercero] ([Id]),
    CONSTRAINT [CK_Rol_Valor] CHECK ([Rol] IN ('Cliente', 'Proveedor', 'Empleado', 'Accionista', 'Acreedor', 'Deudor', 'Otro'))
);
GO

CREATE TABLE [terceros].[CuentaBancariaProveedor]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_CuentaBancariaProveedor] PRIMARY KEY DEFAULT (NEWSEQUENTIALID()),
    [TenantId] uniqueidentifier NOT NULL,
    [TerceroId] uniqueidentifier NOT NULL,
    [Banco] nvarchar(120) NOT NULL,
    [TipoCuenta] varchar(20) NOT NULL,
    [NumeroCuenta] varchar(40) NOT NULL,
    [Activa] bit NOT NULL CONSTRAINT [DF_CuentaBancaria_Activa] DEFAULT (1),
    CONSTRAINT [FK_CuentaBancaria_Tercero] FOREIGN KEY ([TerceroId]) REFERENCES [terceros].[Tercero] ([Id]),
    CONSTRAINT [CK_CuentaBancaria_Tipo] CHECK ([TipoCuenta] IN ('Ahorros', 'Corriente'))
);
GO

CREATE INDEX [IX_CuentaBancaria_Tercero] ON [terceros].[CuentaBancariaProveedor] ([TerceroId]);
GO
