IF SCHEMA_ID(N'tesoreria') IS NULL
    THROW 50001, 'Falta el esquema tesoreria.', 1;

IF OBJECT_ID(N'tesoreria.Conciliacion', 'U') IS NULL
    THROW 50002, 'Falta tesoreria.Conciliacion.', 1;

SELECT CAST(c.[Id] AS nvarchar(50)) + N': conciliacion cerrada con diferencia' AS [Problema]
FROM [tesoreria].[Conciliacion] c
WHERE c.[Estado] = 'Cerrada' AND ABS(c.[Diferencia]) > 0.0000001;

SELECT CAST(t.[Id] AS nvarchar(50)) + N': transferencia con importes incoherentes' AS [Problema]
FROM [tesoreria].[Transferencia] t
INNER JOIN [tesoreria].[Movimiento] ms ON ms.[Id] = t.[MovimientoSalidaId]
INNER JOIN [tesoreria].[Movimiento] me ON me.[Id] = t.[MovimientoEntradaId]
WHERE ms.[Importe] <> t.[Importe] OR me.[Importe] <> t.[Importe];
