/*
===============================================================================
Neros ERP
Script        : V0002__cuenta_sesiones_auditoria.sql
Modulo        : compatibilidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Cuenta y seguridad (fase 2): cambio obligatorio de clave,
                datos de sesion para Mis sesiones y auditoria inmutable.
Dependencias  : V0001__identity_empresas.sql
Objetos       : dbo.AspNetUsers.DebeCambiarClave, dbo.Sesiones (Id, InicioUtc,
                UltimaActividadUtc, Ip, AgenteUsuario, RevocadaEnUtc,
                MotivoRevocacion), esquema auditoria, auditoria.Evento
Motivo        : Plan maestro 15, 16 y 14: restablecimiento por administrador,
                revocacion de sesiones y auditoria con actor.
Impacto       : Columnas nuevas con valor por defecto; las sesiones existentes
                quedan con inicio y actividad igual a la fecha de despliegue.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward. auditoria.Evento es ledger de solo insercion y no
                admite borrado de filas; retirar la tabla exige aprobacion.
Ticket/ADR    : ADR-0004
Validacion    : validation/002_cuenta_sesiones_auditoria.sql
===============================================================================
*/

ALTER TABLE [dbo].[AspNetUsers] ADD
    [DebeCambiarClave] bit NOT NULL CONSTRAINT [DF_AspNetUsers_DebeCambiarClave] DEFAULT (0);
GO

ALTER TABLE [dbo].[Sesiones] ADD
    [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_Sesiones_Id] DEFAULT (NEWID()),
    [InicioUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Sesiones_InicioUtc] DEFAULT (SYSUTCDATETIME()),
    [UltimaActividadUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Sesiones_UltimaActividadUtc] DEFAULT (SYSUTCDATETIME()),
    [Ip] nvarchar(45) NULL,
    [AgenteUsuario] nvarchar(256) NULL,
    [RevocadaEnUtc] datetime2(3) NULL,
    [MotivoRevocacion] varchar(40) NULL;
GO

CREATE UNIQUE INDEX [UX_Sesiones_Id] ON [dbo].[Sesiones] ([Id]);
GO

CREATE SCHEMA [auditoria];
GO

CREATE TABLE [auditoria].[Evento] (
    [Id] uniqueidentifier NOT NULL,
    [FechaUtc] datetime2(3) NOT NULL,
    [Modulo] varchar(40) NOT NULL,
    [Accion] varchar(80) NOT NULL,
    [Resultado] varchar(20) NOT NULL,
    [ActorId] nvarchar(450) NULL,
    [EmpresaId] uniqueidentifier NULL,
    [Entidad] varchar(60) NULL,
    [EntidadId] nvarchar(450) NULL,
    [Ip] nvarchar(45) NULL,
    [AgenteUsuario] nvarchar(256) NULL,
    [CorrelationId] varchar(64) NULL,
    [Detalle] nvarchar(1000) NULL,
    CONSTRAINT [PK_Evento] PRIMARY KEY NONCLUSTERED ([Id]),
    CONSTRAINT [CK_Evento_Resultado] CHECK ([Resultado] IN ('Correcto', 'Rechazado', 'Error'))
)
WITH (LEDGER = ON (APPEND_ONLY = ON));
GO

CREATE CLUSTERED INDEX [IX_Evento_FechaUtc] ON [auditoria].[Evento] ([FechaUtc]);
CREATE INDEX [IX_Evento_ActorId_FechaUtc] ON [auditoria].[Evento] ([ActorId], [FechaUtc]);
CREATE INDEX [IX_Evento_Entidad_EntidadId_FechaUtc] ON [auditoria].[Evento] ([Entidad], [EntidadId], [FechaUtc]);
GO
