/*
Validacion post-despliegue del modulo inventario.
*/

IF SCHEMA_ID(N'inventario') IS NULL
    THROW 50001, 'Falta el esquema inventario.', 1;

IF OBJECT_ID(N'inventario.MovimientoInventario', 'U') IS NULL
    THROW 50002, 'Falta la tabla inventario.MovimientoInventario.', 1;

IF OBJECT_ID(N'inventario.Existencia', 'U') IS NULL
    THROW 50003, 'Falta la tabla inventario.Existencia.', 1;

SELECT CAST(e.[ProductoId] AS nvarchar(50)) + N'/' + CAST(e.[BodegaId] AS nvarchar(50)) + N': cantidad negativa en existencia' AS [Problema]
FROM [inventario].[Existencia] e
INNER JOIN [inventario].[Bodega] b ON b.[Id] = e.[BodegaId]
WHERE e.[Cantidad] < 0 AND b.[PermitirExistenciasNegativas] = 0;
