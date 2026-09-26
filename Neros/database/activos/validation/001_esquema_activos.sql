/*
Validacion post-despliegue del modulo activos.
*/

IF SCHEMA_ID(N'activos') IS NULL
    THROW 50001, 'Falta el esquema activos.', 1;

IF OBJECT_ID(N'activos.ActivoFijo', 'U') IS NULL
    THROW 50002, 'Falta activos.ActivoFijo.', 1;

SELECT CAST(a.[Id] AS nvarchar(50)) + N': valor en libros incoherente' AS [Problema]
FROM [activos].[ActivoFijo] a
WHERE a.[Estado] <> 'DadoDeBaja'
  AND a.[ValorEnLibros] <> (a.[CostoAdquisicion] - a.[DepreciacionAcumulada] - a.[DeterioroAcumulado]);

SELECT CAST(a.[Id] AS nvarchar(50)) + N': activo sin cuota en periodo contabilizado' AS [Problema]
FROM [activos].[ActivoFijo] a
INNER JOIN [activos].[DepreciacionPeriodo] d ON d.[ActivoFijoId] = a.[Id]
WHERE d.[Estado] = 'Contabilizada' AND d.[ContabilizadaEnUtc] IS NULL;

SELECT CAST(d.[Id] AS nvarchar(50)) + N': depreciacion supera base depreciable' AS [Problema]
FROM [activos].[DepreciacionPeriodo] d
INNER JOIN [activos].[ActivoFijo] a ON a.[Id] = d.[ActivoFijoId]
WHERE d.[DepreciacionAcumulada] > (a.[CostoAdquisicion] - a.[ValorResidual]);
