/*
Validacion post-despliegue del modulo nomina.
*/

IF SCHEMA_ID(N'nomina') IS NULL
    THROW 50001, 'Falta el esquema nomina.', 1;

IF OBJECT_ID(N'nomina.LiquidacionNomina', 'U') IS NULL
    THROW 50002, 'Falta la tabla nomina.LiquidacionNomina.', 1;

SELECT CAST(e.[Id] AS nvarchar(50)) + N': mas de un contrato activo' AS [Problema]
FROM [nomina].[Empleado] e
WHERE (
    SELECT COUNT(*)
    FROM [nomina].[Contrato] c
    WHERE c.[EmpleadoId] = e.[Id] AND c.[Estado] = 'Activo'
) > 1;

SELECT CAST(l.[Id] AS nvarchar(50)) + N': calculada sin snapshot' AS [Problema]
FROM [nomina].[LiquidacionNomina] l
WHERE l.[Estado] IN ('Calculada', 'Contabilizada')
  AND NOT EXISTS (SELECT 1 FROM [nomina].[LiquidacionSnapshot] s WHERE s.[LiquidacionNominaId] = l.[Id]);

SELECT CAST(l.[Id] AS nvarchar(50)) + N': calculada sin lineas' AS [Problema]
FROM [nomina].[LiquidacionNomina] l
WHERE l.[Estado] IN ('Calculada', 'Contabilizada')
  AND NOT EXISTS (SELECT 1 FROM [nomina].[LiquidacionLinea] ln WHERE ln.[LiquidacionNominaId] = l.[Id]);

SELECT CAST(l.[Id] AS nvarchar(50)) + N': neto distinto a devengos menos deducciones' AS [Problema]
FROM [nomina].[LiquidacionNomina] l
WHERE l.[Estado] IN ('Calculada', 'Contabilizada')
  AND l.[NetoPagar] <> (l.[TotalDevengos] - l.[TotalDeducciones]);

SELECT CAST(l.[Id] AS nvarchar(50)) + N': contabilizada sin marca temporal' AS [Problema]
FROM [nomina].[LiquidacionNomina] l
WHERE l.[Estado] = 'Contabilizada' AND l.[ContabilizadaEnUtc] IS NULL;
