SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetUsers] (
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


CREATE TABLE [Empresas] (
    [Id] uniqueidentifier NOT NULL,
    [Codigo] nvarchar(20) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Identificacion] nvarchar(30) NOT NULL,
    [Activa] bit NOT NULL,
    CONSTRAINT [PK_Empresas] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Sesiones] (
    [TokenHash] varchar(64) NOT NULL,
    [UsuarioId] nvarchar(450) NOT NULL,
    [SelloSeguridad] nvarchar(256) NOT NULL,
    [Expira] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Sesiones] PRIMARY KEY ([TokenHash]),
    CONSTRAINT [FK_Sesiones_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [EventosAcceso] (
    [Id] bigint NOT NULL IDENTITY,
    [UsuarioId] nvarchar(450) NULL,
    [EmpresaId] uniqueidentifier NULL,
    [Fecha] datetimeoffset NOT NULL,
    [Accion] nvarchar(80) NOT NULL,
    CONSTRAINT [PK_EventosAcceso] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventosAcceso_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventosAcceso_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [Empresas] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [UsuariosEmpresas] (
    [UsuarioId] nvarchar(450) NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Rol] nvarchar(30) NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_UsuariosEmpresas] PRIMARY KEY ([UsuarioId], [EmpresaId]),
    CONSTRAINT [FK_UsuariosEmpresas_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UsuariosEmpresas_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [Empresas] ([Id]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_Empresas_Codigo] ON [Empresas] ([Codigo]);
GO


CREATE INDEX [IX_EventosAcceso_EmpresaId] ON [EventosAcceso] ([EmpresaId]);
GO


CREATE INDEX [IX_EventosAcceso_UsuarioId_EmpresaId_Fecha] ON [EventosAcceso] ([UsuarioId], [EmpresaId], [Fecha]);
GO


CREATE INDEX [IX_Sesiones_Expira] ON [Sesiones] ([Expira]);
GO


CREATE INDEX [IX_Sesiones_UsuarioId] ON [Sesiones] ([UsuarioId]);
GO


CREATE INDEX [IX_UsuariosEmpresas_EmpresaId] ON [UsuariosEmpresas] ([EmpresaId]);
GO

COMMIT TRANSACTION;
GO



