/*
Validacion post-despliegue del modulo busqueda (D-17b).
*/

IF SCHEMA_ID(N'busqueda') IS NULL
    THROW 50026, 'Falta el esquema busqueda.', 1;

IF OBJECT_ID(N'busqueda.DocumentoIndice', N'U') IS NULL
    THROW 50026, 'Falta busqueda.DocumentoIndice.', 1;

SELECT CAST(d.[Id] AS nvarchar(50)) + N': tombstone activo inconsistente' AS [Problema]
FROM [busqueda].[DocumentoIndice] d
WHERE d.[TombstoneEnUtc] IS NOT NULL AND d.[Activo] = 1;
