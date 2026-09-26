/*
Validacion post-despliegue del modulo contabilidad.
*/

IF SCHEMA_ID(N'contabilidad') IS NULL
    THROW 50001, 'Falta el esquema contabilidad.', 1;

IF OBJECT_ID(N'contabilidad.Comprobante', 'U') IS NULL
    THROW 50002, 'Falta la tabla contabilidad.Comprobante.', 1;

IF OBJECT_ID(N'contabilidad.MovimientoContable', 'U') IS NULL
    THROW 50003, 'Falta la tabla contabilidad.MovimientoContable.', 1;

SELECT CAST(c.[Id] AS nvarchar(50)) + N': comprobante contabilizado descuadrado' AS [Problema]
FROM [contabilidad].[Comprobante] c
INNER JOIN (
    SELECT [ComprobanteId], SUM([Debito]) AS [TotalDebito], SUM([Credito]) AS [TotalCredito]
    FROM [contabilidad].[MovimientoContable]
    GROUP BY [ComprobanteId]
) t ON t.[ComprobanteId] = c.[Id]
WHERE c.[Estado] = 'Contabilizado'
  AND ABS(t.[TotalDebito] - t.[TotalCredito]) > 0.0001;
