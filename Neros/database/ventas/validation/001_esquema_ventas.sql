/*
Validacion post-despliegue del modulo ventas.
*/

IF SCHEMA_ID(N'ventas') IS NULL
    THROW 50001, 'Falta el esquema ventas.', 1;

IF OBJECT_ID(N'ventas.Cotizacion', 'U') IS NULL
    THROW 50002, 'Falta la tabla ventas.Cotizacion.', 1;

IF OBJECT_ID(N'ventas.Pedido', 'U') IS NULL
    THROW 50003, 'Falta la tabla ventas.Pedido.', 1;

SELECT CAST(c.[Id] AS nvarchar(50)) + N': confirmada sin snapshot de impuestos' AS [Problema]
FROM [ventas].[Cotizacion] c
WHERE c.[Estado] = 'Confirmado'
  AND (c.[ImpuestosVersionPublicadaId] IS NULL OR c.[ImpuestosVersionNumero] IS NULL OR c.[ClienteRazonSocial] IS NULL);

SELECT CAST(p.[Id] AS nvarchar(50)) + N': confirmado sin snapshot de impuestos' AS [Problema]
FROM [ventas].[Pedido] p
WHERE p.[Estado] = 'Confirmado'
  AND (p.[ImpuestosVersionPublicadaId] IS NULL OR p.[ImpuestosVersionNumero] IS NULL OR p.[ClienteRazonSocial] IS NULL);
