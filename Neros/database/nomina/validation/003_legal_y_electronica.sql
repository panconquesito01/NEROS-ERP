/*
Validacion paquete legal y nomina electronica.
*/

IF OBJECT_ID(N'nomina.PaqueteLegalNomina', 'U') IS NULL
    THROW 50020, 'Falta nomina.PaqueteLegalNomina.', 1;

IF OBJECT_ID(N'nomina.NominaElectronica', 'U') IS NULL
    THROW 50021, 'Falta nomina.NominaElectronica.', 1;

SELECT CAST(p.[Id] AS nvarchar(50)) + N': aprobado sin marca de especialista' AS [Problema]
FROM [nomina].[PaqueteLegalNomina] p
WHERE p.[EstadoAprobacion] = 'Aprobado' AND p.[AprobadoPorEspecialistaEnUtc] IS NULL;

SELECT CAST(n.[Id] AS nvarchar(50)) + N': validada sin CUNE' AS [Problema]
FROM [nomina].[NominaElectronica] n
WHERE n.[Estado] IN ('Validado', 'Entregado') AND (n.[Cune] IS NULL OR LTRIM(RTRIM(n.[Cune])) = '');

SELECT CAST(l.[Id] AS nvarchar(50)) + N': contabilizada sin nomina electronica' AS [Problema]
FROM [nomina].[LiquidacionNomina] l
WHERE l.[Estado] = 'Contabilizada'
  AND NOT EXISTS (SELECT 1 FROM [nomina].[NominaElectronica] e WHERE e.[LiquidacionNominaId] = l.[Id]);

SELECT CAST(l.[Id] AS nvarchar(50)) + N': legal Colombia sin fila de seguridad social' AS [Problema]
FROM [nomina].[LiquidacionNomina] l
INNER JOIN [nomina].[LiquidacionSnapshot] s ON s.[LiquidacionNominaId] = l.[Id]
WHERE l.[Estado] IN ('Calculada', 'Contabilizada')
  AND s.[AplicaReglasLegalesColombia] = 1
  AND NOT EXISTS (SELECT 1 FROM [nomina].[LiquidacionSeguridadSocial] ss WHERE ss.[LiquidacionNominaId] = l.[Id]);
