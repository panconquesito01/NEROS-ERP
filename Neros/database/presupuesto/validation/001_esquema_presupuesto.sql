/*
Validacion post-despliegue del modulo presupuesto.
*/

IF SCHEMA_ID(N'presupuesto') IS NULL
    THROW 50001, 'Falta el esquema presupuesto.', 1;

IF OBJECT_ID(N'presupuesto.LineaPresupuesto', 'U') IS NULL
    THROW 50002, 'Falta presupuesto.LineaPresupuesto.', 1;

SELECT CAST(v.[Id] AS nvarchar(50)) + N': linea fuera del anio de la version' AS [Problema]
FROM [presupuesto].[VersionPresupuesto] v
INNER JOIN [presupuesto].[LineaPresupuesto] l ON l.[VersionPresupuestoId] = v.[Id]
WHERE l.[Anio] <> v.[Anio];

SELECT CAST(l.[Id] AS nvarchar(50)) + N': presupuesto en version no aprobada' AS [Problema]
FROM [presupuesto].[LineaPresupuesto] l
INNER JOIN [presupuesto].[VersionPresupuesto] v ON v.[Id] = l.[VersionPresupuestoId]
WHERE v.[Estado] = 'Borrador' AND l.[ValorPresupuestado] > 0;
