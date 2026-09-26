/*
===============================================================================
Neros ERP
Script        : S0001__empresas_demostracion.sql
Modulo        : compatibilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Tres empresas de demostracion para pruebas multiempresa.
Dependencias  : V0001__identity_empresas.sql
Objetos       : dbo.Empresas, dbo.UsuariosEmpresas
Motivo        : Entornos locales y pruebas de cambio de empresa.
Impacto       : Inserta empresas y membresias de administrador.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

MERGE [dbo].[Empresas] AS destino
USING (VALUES
    ('11111111-1111-4111-8111-111111110001', N'RETAIL01', N'Neros Retail Demo', N'900111001-1', 1),
    ('11111111-1111-4111-8111-111111110002', N'MANUF01', N'Andina Manufactura Demo', N'900222002-2', 1),
    ('11111111-1111-4111-8111-111111110003', N'SERVIC01', N'Pacífico Servicios Demo', N'900333003-3', 1)
) AS origen ([Id], [Codigo], [Nombre], [Identificacion], [Activa])
ON destino.[Codigo] = origen.[Codigo]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Codigo], [Nombre], [Identificacion], [Activa])
    VALUES (origen.[Id], origen.[Codigo], origen.[Nombre], origen.[Identificacion], origen.[Activa])
WHEN MATCHED AND (destino.[Nombre] <> origen.[Nombre] OR destino.[Identificacion] <> origen.[Identificacion] OR destino.[Activa] <> origen.[Activa]) THEN
    UPDATE SET [Nombre] = origen.[Nombre], [Identificacion] = origen.[Identificacion], [Activa] = origen.[Activa];
GO

;WITH [EmpresasDemo] AS (
    SELECT [Id] FROM [dbo].[Empresas] WHERE [Codigo] IN (N'RETAIL01', N'MANUF01', N'SERVIC01')
),
[AdministradoresGlobales] AS (
    SELECT DISTINCT u.[Id]
    FROM [dbo].[AspNetUsers] u
    INNER JOIN [dbo].[AspNetUserClaims] c ON c.[UserId] = u.[Id]
    WHERE c.[ClaimType] = N'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      AND c.[ClaimValue] = N'AdministradorGlobal'
      AND u.[Activo] = 1
),
[UsuariosConMembresia] AS (
    SELECT DISTINCT u.[Id]
    FROM [dbo].[AspNetUsers] u
    INNER JOIN [dbo].[UsuariosEmpresas] ue ON ue.[UsuarioId] = u.[Id] AND ue.[Activo] = 1
    WHERE u.[Activo] = 1
)
INSERT INTO [dbo].[UsuariosEmpresas] ([UsuarioId], [EmpresaId], [Rol], [Activo])
SELECT u.[Id], e.[Id], N'Administrador', 1
FROM [EmpresasDemo] e
CROSS JOIN (
    SELECT [Id] FROM [AdministradoresGlobales]
    UNION
    SELECT [Id] FROM [UsuariosConMembresia]
) u
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[UsuariosEmpresas] x
    WHERE x.[UsuarioId] = u.[Id] AND x.[EmpresaId] = e.[Id]
);
GO
