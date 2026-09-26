/*
===============================================================================
Neros ERP
Script        : S0003__definiciones_cookies.sql
Modulo        : privacidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Inventario de cookies y almacenamiento local usados hoy.
Dependencias  : V0001__esquema_privacidad.sql
Objetos       : privacidad.DefinicionCookie
Motivo        : Plan maestro §25; pagina /cookies.
Impacto       : Catalogo publico; sin banner hasta cookies no esenciales.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

MERGE [privacidad].[DefinicionCookie] AS destino
USING (VALUES
    (N'Neros.Sesion', 'Cookie', N'Neros', 'Esencial', N'Mantener la sesion autenticada del usuario en el BFF.', N'Sesion o hasta 7 dias si se elige recordarme', 1, NULL, 1, N'/cookies', 10, 1),
    (N'.AspNetCore.Antiforgery', 'Cookie', N'Neros', 'Esencial', N'Proteger formularios contra falsificacion (CSRF).', N'Sesion del navegador', 1, NULL, 1, N'/cookies', 20, 1),
    (N'.AspNetCore.Culture', 'Cookie', N'Neros', 'Preferencia', N'Recordar el idioma elegido en la interfaz.', N'1 ano', 1, NULL, 0, N'/cookies', 30, 1),
    (N'neros.theme', 'LocalStorage', N'Neros', 'Preferencia', N'Recordar tema claro, oscuro o del sistema.', N'Hasta que el usuario borre datos del sitio', 1, NULL, 0, N'/cookies', 40, 1),
    (N'neros.remembered-email', 'LocalStorage', N'Neros', 'Preferencia', N'Recordar el correo en login solo si el usuario marca la casilla.', N'Hasta desmarcar o borrar datos del sitio', 1, NULL, 0, N'/cookies', 50, 1),
    (N'neros.input', 'LocalStorage', N'Neros', 'Preferencia', N'Recordar si la ultima interaccion fue teclado o puntero (accesibilidad).', N'Sesion del navegador', 1, NULL, 0, N'/cookies', 60, 1)
) AS origen ([Nombre], [Almacenamiento], [Proveedor], [Categoria], [Finalidad], [Duracion], [PrimeraParte], [Dominio], [Esencial], [UrlPolitica], [Orden], [Activo])
ON destino.[Nombre] = origen.[Nombre] AND destino.[Almacenamiento] = origen.[Almacenamiento]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Nombre], [Almacenamiento], [Proveedor], [Categoria], [Finalidad], [Duracion], [PrimeraParte], [Dominio], [Esencial], [UrlPolitica], [Orden], [Activo])
    VALUES (origen.[Nombre], origen.[Almacenamiento], origen.[Proveedor], origen.[Categoria], origen.[Finalidad], origen.[Duracion],
        origen.[PrimeraParte], origen.[Dominio], origen.[Esencial], origen.[UrlPolitica], origen.[Orden], origen.[Activo]);
GO
