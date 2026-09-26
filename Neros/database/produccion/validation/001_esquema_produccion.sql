/*
Validacion post-despliegue del modulo produccion.
*/

IF SCHEMA_ID(N'produccion') IS NULL
    THROW 50001, 'Falta el esquema produccion.', 1;

IF OBJECT_ID(N'produccion.OrdenProduccion', 'U') IS NULL
    THROW 50002, 'Falta produccion.OrdenProduccion.', 1;

SELECT CAST(o.[Id] AS nvarchar(50)) + N': terminada supera planificada' AS [Problema]
FROM [produccion].[OrdenProduccion] o
WHERE o.[CantidadTerminada] + o.[CantidadMerma] > o.[CantidadPlanificada];

SELECT CAST(c.[OrdenProduccionId] AS nvarchar(50)) + N': costo total incoherente' AS [Problema]
FROM [produccion].[CostoOrdenProduccion] c
WHERE c.[CostoTotal] <> (c.[CostoMateriaPrima] + c.[CostoManoObra] + c.[CostoIndirecto]);

SELECT CAST(m.[Id] AS nvarchar(50)) + N': consumo sin componente' AS [Problema]
FROM [produccion].[MovimientoProduccion] m
WHERE m.[TipoMovimiento] IN ('Consumo', 'Devolucion') AND m.[ComponenteReferenciaId] IS NULL;
