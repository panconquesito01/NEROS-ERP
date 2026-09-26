/*
===============================================================================
Neros ERP
Script        : S0002__permisos_desarrollo.sql
Modulo        : compatibilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Administrador global y modulos completos para cuentas activas (dev).
Dependencias  : S0001__empresas_demostracion.sql
Objetos       : dbo.AspNetUserClaims, dbo.UsuariosEmpresas
Motivo        : Probar plataforma y todos los modulos operativos en local.
Impacto       : Concede rol global y JSON de modulos en membresias existentes.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO (solo datos de permisos; no borra cuentas)
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

DECLARE @RolGlobal nvarchar(64) = N'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
DECLARE @Modulos nvarchar(500) = N'["Ventas","Inventarios","Compras","Finanzas","Informes","Multiempresa"]';

INSERT INTO [dbo].[AspNetUserClaims] ([UserId], [ClaimType], [ClaimValue])
SELECT u.[Id], @RolGlobal, N'AdministradorGlobal'
FROM [dbo].[AspNetUsers] u
WHERE u.[Activo] = 1
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[AspNetUserClaims] c
      WHERE c.[UserId] = u.[Id] AND c.[ClaimType] = @RolGlobal AND c.[ClaimValue] = N'AdministradorGlobal'
  );
GO

DECLARE @ModulosMembresia nvarchar(500) = N'["Ventas","Inventarios","Compras","Finanzas","Informes","Multiempresa"]';

UPDATE ue
SET [Rol] = N'Administrador',
    [ModulosHabilitados] = @ModulosMembresia
FROM [dbo].[UsuariosEmpresas] ue
INNER JOIN [dbo].[AspNetUsers] u ON u.[Id] = ue.[UsuarioId]
WHERE u.[Activo] = 1
  AND ue.[Activo] = 1
  AND (ue.[ModulosHabilitados] IS NULL OR ue.[ModulosHabilitados] <> @ModulosMembresia OR ue.[Rol] <> N'Administrador');
GO
