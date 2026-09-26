/*
===============================================================================
Neros ERP
Script        : V0001__identity_empresas.sql
Modulo        : compatibilidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Esquema inicial del host de compatibilidad: ASP.NET Identity,
                empresas, membresias, sesiones opacas y eventos de acceso.
Dependencias  : Ninguna
Objetos       : dbo.AspNetUsers, dbo.AspNetUserClaims, dbo.AspNetUserLogins,
                dbo.AspNetUserTokens, dbo.Empresas, dbo.UsuariosEmpresas,
                dbo.Sesiones, dbo.EventosAcceso
Motivo        : Adoptar el antiguo database/scripts/001_identity_empresas.sql
                bajo el runner (ADR-0004). Las bases existentes usan baseline.
Impacto       : Ninguno en bases existentes (baseline --hasta V0001).
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Base nueva: eliminar la base. Base existente: no aplica (baseline).
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_compatibilidad.sql
===============================================================================
*/

CREATE TABLE [dbo].[AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Activo] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [dbo].[Empresas] (
    [Id] uniqueidentifier NOT NULL,
    [Codigo] nvarchar(20) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Identificacion] nvarchar(30) NOT NULL,
    [Activa] bit NOT NULL,
    CONSTRAINT [PK_Empresas] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[Sesiones] (
    [TokenHash] varchar(64) NOT NULL,
    [UsuarioId] nvarchar(450) NOT NULL,
    [SelloSeguridad] nvarchar(256) NOT NULL,
    [Expira] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Sesiones] PRIMARY KEY ([TokenHash]),
    CONSTRAINT [FK_Sesiones_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[EventosAcceso] (
    [Id] bigint NOT NULL IDENTITY,
    [UsuarioId] nvarchar(450) NULL,
    [EmpresaId] uniqueidentifier NULL,
    [Fecha] datetimeoffset NOT NULL,
    [Accion] nvarchar(80) NOT NULL,
    CONSTRAINT [PK_EventosAcceso] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventosAcceso_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventosAcceso_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [dbo].[Empresas] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [dbo].[UsuariosEmpresas] (
    [UsuarioId] nvarchar(450) NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Rol] nvarchar(30) NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_UsuariosEmpresas] PRIMARY KEY ([UsuarioId], [EmpresaId]),
    CONSTRAINT [FK_UsuariosEmpresas_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UsuariosEmpresas_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [dbo].[Empresas] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
CREATE UNIQUE INDEX [IX_Empresas_Codigo] ON [dbo].[Empresas] ([Codigo]);
CREATE INDEX [IX_EventosAcceso_EmpresaId] ON [dbo].[EventosAcceso] ([EmpresaId]);
CREATE INDEX [IX_EventosAcceso_UsuarioId_EmpresaId_Fecha] ON [dbo].[EventosAcceso] ([UsuarioId], [EmpresaId], [Fecha]);
CREATE INDEX [IX_Sesiones_Expira] ON [dbo].[Sesiones] ([Expira]);
CREATE INDEX [IX_Sesiones_UsuarioId] ON [dbo].[Sesiones] ([UsuarioId]);
CREATE INDEX [IX_UsuariosEmpresas_EmpresaId] ON [dbo].[UsuariosEmpresas] ([EmpresaId]);
GO
