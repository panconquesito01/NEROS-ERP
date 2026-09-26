IF SCHEMA_ID(N'cartera') IS NULL
    THROW 50001, 'Falta el esquema cartera.', 1;

IF OBJECT_ID(N'cartera.Movimiento', 'U') IS NULL
    THROW 50002, 'Falta la tabla cartera.Movimiento.', 1;

SELECT CAST(d.[Id] AS nvarchar(50)) + N': proyeccion distinta a movimientos' AS [Problema]
FROM [cartera].[Documento] d
INNER JOIN [cartera].[ProyeccionSaldo] p ON p.[DocumentoId] = d.[Id]
CROSS APPLY (
    SELECT
        ISNULL(SUM(CASE WHEN m.[Naturaleza] = 'Cargo' THEN m.[Importe] ELSE 0 END), 0)
        - ISNULL(SUM(CASE WHEN m.[Naturaleza] = 'Abono' THEN m.[Importe] ELSE 0 END), 0) AS [SaldoCalculado]
    FROM [cartera].[Movimiento] m
    WHERE m.[DocumentoId] = d.[Id]
) c
WHERE ABS(p.[Saldo] - c.[SaldoCalculado]) > 0.0000001;

SELECT CAST(p.[Id] AS nvarchar(50)) + N': aplicaciones + anticipo != importe pago' AS [Problema]
FROM [cartera].[Pago] p
CROSS APPLY (
    SELECT ISNULL(SUM(a.[ImporteAplicado]), 0) AS [Aplicado]
    FROM [cartera].[AplicacionPago] a
    WHERE a.[PagoId] = p.[Id]
) s
WHERE ABS(p.[ImportePago] - (s.[Aplicado] + p.[ImporteAnticipo])) > 0.0000001;
