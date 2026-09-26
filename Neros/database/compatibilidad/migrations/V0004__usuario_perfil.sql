/*
===============================================================================
Neros ERP
Script        : V0004__usuario_perfil.sql
Modulo        : compatibilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Perfil extendido de usuario y modulos habilitados por membresia.
Dependencias  : V0001__identity_empresas.sql
Objetos       : columnas en AspNetUsers, UsuariosEmpresas
Motivo        : Alta de usuarios con identidad, ubicacion y modulos por empresa.
Impacto       : Columnas nullable; sin cambio de filas existentes.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/004_usuario_perfil.sql
===============================================================================
*/

ALTER TABLE [dbo].[AspNetUsers]
    ADD [PrimerNombre] nvarchar(80) NULL,
        [SegundoNombre] nvarchar(80) NULL,
        [PrimerApellido] nvarchar(80) NULL,
        [SegundoApellido] nvarchar(80) NULL,
        [TipoDocumento] varchar(10) NULL,
        [NumeroDocumento] varchar(30) NULL,
        [Ciudad] nvarchar(120) NULL,
        [Direccion] nvarchar(256) NULL;
GO

ALTER TABLE [dbo].[UsuariosEmpresas]
    ADD [ModulosHabilitados] nvarchar(500) NULL;
GO
