/*
Validacion post-despliegue del modulo analitica (D-17a).
*/

IF SCHEMA_ID(N'analitica') IS NULL
    THROW 50025, 'Falta el esquema analitica.', 1;

IF OBJECT_ID(N'analitica.EventoIngesta', N'U') IS NULL
    THROW 50025, 'Falta analitica.EventoIngesta.', 1;

IF OBJECT_ID(N'analitica.HechoOperativo', N'U') IS NULL
    THROW 50025, 'Falta analitica.HechoOperativo.', 1;

SELECT CAST(e.[Id] AS nvarchar(50)) + N': hecho huerfano sin evento' AS [Problema]
FROM [analitica].[HechoOperativo] h
LEFT JOIN [analitica].[EventoIngesta] e ON e.[Id] = h.[EventoIngestaId]
WHERE e.[Id] IS NULL;
