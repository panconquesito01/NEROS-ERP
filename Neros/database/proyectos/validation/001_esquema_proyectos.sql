/*
Validacion post-despliegue del modulo proyectos.
*/

IF SCHEMA_ID(N'proyectos') IS NULL
    THROW 50001, 'Falta el esquema proyectos.', 1;

IF OBJECT_ID(N'proyectos.Proyecto', 'U') IS NULL
    THROW 50002, 'Falta proyectos.Proyecto.', 1;

SELECT CAST(p.[Id] AS nvarchar(50)) + N': referencia a proyecto cerrado o cancelado' AS [Problema]
FROM [proyectos].[Proyecto] p
INNER JOIN [proyectos].[ReferenciaMovimientoProyecto] r ON r.[ProyectoId] = p.[Id]
WHERE p.[Estado] IN ('Cerrado', 'Cancelado')
  AND r.[RegistradoEnUtc] > DATEADD(day, -1, sysutcdatetime());

SELECT CAST(p.[Id] AS nvarchar(50)) + N': ejecucion supera presupuesto total' AS [Problema]
FROM [proyectos].[Proyecto] p
WHERE p.[PresupuestoTotal] > 0
  AND (
      SELECT ISNULL(SUM(r.[ImporteAsignado]), 0)
      FROM [proyectos].[ReferenciaMovimientoProyecto] r
      WHERE r.[ProyectoId] = p.[Id]
  ) > p.[PresupuestoTotal];
