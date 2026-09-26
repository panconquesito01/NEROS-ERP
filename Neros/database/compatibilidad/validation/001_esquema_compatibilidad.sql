-- Devuelve una fila por cada tabla o indice esperado que falte. Sin filas = esquema correcto.
WITH Esperados (Tipo, Tabla, Nombre) AS
(
    SELECT Tipo, Tabla, Nombre
    FROM (VALUES
        ('Tabla', N'AspNetUsers', NULL), ('Tabla', N'AspNetUserClaims', NULL),
        ('Tabla', N'AspNetUserLogins', NULL), ('Tabla', N'AspNetUserTokens', NULL),
        ('Tabla', N'Empresas', NULL), ('Tabla', N'UsuariosEmpresas', NULL),
        ('Tabla', N'Sesiones', NULL), ('Tabla', N'EventosAcceso', NULL),
        ('Indice', N'AspNetUserClaims', N'IX_AspNetUserClaims_UserId'),
        ('Indice', N'AspNetUserLogins', N'IX_AspNetUserLogins_UserId'),
        ('Indice', N'AspNetUsers', N'EmailIndex'),
        ('Indice', N'AspNetUsers', N'UserNameIndex'),
        ('Indice', N'Empresas', N'IX_Empresas_Codigo'),
        ('Indice', N'EventosAcceso', N'IX_EventosAcceso_EmpresaId'),
        ('Indice', N'EventosAcceso', N'IX_EventosAcceso_UsuarioId_EmpresaId_Fecha'),
        ('Indice', N'Sesiones', N'IX_Sesiones_Expira'),
        ('Indice', N'Sesiones', N'IX_Sesiones_UsuarioId'),
        ('Indice', N'UsuariosEmpresas', N'IX_UsuariosEmpresas_EmpresaId')
    ) AS Valores (Tipo, Tabla, Nombre)
)
SELECT CONCAT(e.Tipo, N' ausente: dbo.', e.Tabla, CASE WHEN e.Nombre IS NULL THEN N'' ELSE CONCAT(N'.', e.Nombre) END) AS Problema
FROM Esperados AS e
WHERE (e.Tipo = 'Tabla' AND OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(e.Tabla), N'U') IS NULL)
   OR (e.Tipo = 'Indice' AND NOT EXISTS (
        SELECT 1 FROM sys.indexes AS i
        WHERE i.object_id = OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(e.Tabla), N'U') AND i.name = e.Nombre));
