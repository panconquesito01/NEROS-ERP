/*
Validacion post-despliegue del modulo compras.
*/

IF SCHEMA_ID(N'compras') IS NULL
    THROW 50001, 'Falta el esquema compras.', 1;

IF OBJECT_ID(N'compras.OrdenCompra', 'U') IS NULL
    THROW 50002, 'Falta la tabla compras.OrdenCompra.', 1;

IF OBJECT_ID(N'compras.RecepcionCompra', 'U') IS NULL
    THROW 50003, 'Falta la tabla compras.RecepcionCompra.', 1;

SELECT CAST(o.[Id] AS nvarchar(50)) + N': aprobada sin snapshot de impuestos' AS [Problema]
FROM [compras].[OrdenCompra] o
WHERE o.[Estado] IN ('Aprobada', 'RecibidaParcial', 'RecibidaTotal')
  AND (o.[ImpuestosVersionPublicadaId] IS NULL OR o.[ImpuestosVersionNumero] IS NULL OR o.[ProveedorRazonSocial] IS NULL);

SELECT CAST(o.[Id] AS nvarchar(50)) + N': aprobada sin aprobador' AS [Problema]
FROM [compras].[OrdenCompra] o
WHERE o.[Estado] IN ('Aprobada', 'RecibidaParcial', 'RecibidaTotal')
  AND (o.[AprobadoPorUsuarioId] IS NULL OR o.[AprobadoEnUtc] IS NULL);

SELECT CAST(o.[Id] AS nvarchar(50)) + N': aprobador igual al creador' AS [Problema]
FROM [compras].[OrdenCompra] o
WHERE o.[AprobadoPorUsuarioId] IS NOT NULL AND o.[AprobadoPorUsuarioId] = o.[CreadoPorUsuarioId];

SELECT CAST(l.[Id] AS nvarchar(50)) + N': recibida supera pedida' AS [Problema]
FROM [compras].[OrdenCompraLinea] l
WHERE l.[CantidadRecibidaAcumulada] > l.[CantidadPedida];
