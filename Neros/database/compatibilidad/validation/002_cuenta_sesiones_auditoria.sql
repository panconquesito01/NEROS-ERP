-- Devuelve una fila por cada columna, indice o propiedad de V0002 que falte. Sin filas = correcto.
WITH Columnas (Tabla, Columna) AS
(
    SELECT Tabla, Columna
    FROM (VALUES
        (N'dbo.AspNetUsers', N'DebeCambiarClave'),
        (N'dbo.Sesiones', N'Id'), (N'dbo.Sesiones', N'InicioUtc'), (N'dbo.Sesiones', N'UltimaActividadUtc'),
        (N'dbo.Sesiones', N'Ip'), (N'dbo.Sesiones', N'AgenteUsuario'), (N'dbo.Sesiones', N'RevocadaEnUtc'),
        (N'dbo.Sesiones', N'MotivoRevocacion')
    ) AS Valores (Tabla, Columna)
)
SELECT CONCAT(N'Columna ausente: ', c.Tabla, N'.', c.Columna) AS Problema
FROM Columnas AS c
WHERE COL_LENGTH(c.Tabla, c.Columna) IS NULL
UNION ALL
SELECT N'Indice ausente: dbo.Sesiones.UX_Sesiones_Id'
WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sesiones') AND name = N'UX_Sesiones_Id' AND is_unique = 1)
UNION ALL
SELECT N'auditoria.Evento no existe o no es ledger de solo insercion'
WHERE NOT EXISTS (
    SELECT 1 FROM sys.tables AS t
    WHERE t.object_id = OBJECT_ID(N'auditoria.Evento') AND t.ledger_type_desc = N'APPEND_ONLY_LEDGER_TABLE');
