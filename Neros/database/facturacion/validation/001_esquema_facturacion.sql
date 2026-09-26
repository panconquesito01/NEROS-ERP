/*
Validacion post-despliegue del modulo facturacion.
*/

IF SCHEMA_ID(N'facturacion') IS NULL
    THROW 50001, 'Falta el esquema facturacion.', 1;

IF OBJECT_ID(N'facturacion.NumeracionDocumento', 'U') IS NULL
    THROW 50002, 'Falta la tabla facturacion.NumeracionDocumento.', 1;

IF OBJECT_ID(N'facturacion.DocumentoFiscal', 'U') IS NULL
    THROW 50003, 'Falta la tabla facturacion.DocumentoFiscal.', 1;

IF OBJECT_ID(N'facturacion.DocumentoElectronico', 'U') IS NULL
    THROW 50004, 'Falta la tabla facturacion.DocumentoElectronico.', 1;

SELECT CAST(n.[Id] AS nvarchar(50)) + N': Actual fuera de rango' AS [Problema]
FROM [facturacion].[NumeracionDocumento] n
WHERE n.[Actual] < n.[Desde] - 1 OR n.[Actual] > n.[Hasta];

SELECT CAST(c.[Id] AS nvarchar(50)) + N': emitido sin documento fiscal' AS [Problema]
FROM [facturacion].[DocumentoComercial] c
WHERE c.[Estado] = 'Emitido'
  AND NOT EXISTS (SELECT 1 FROM [facturacion].[DocumentoFiscal] f WHERE f.[DocumentoComercialId] = c.[Id]);

SELECT CAST(f.[Id] AS nvarchar(50)) + N': fiscal sin electronico' AS [Problema]
FROM [facturacion].[DocumentoFiscal] f
WHERE NOT EXISTS (SELECT 1 FROM [facturacion].[DocumentoElectronico] e WHERE e.[DocumentoFiscalId] = f.[Id]);

SELECT CAST(e.[Id] AS nvarchar(50)) + N': validado sin CUFE' AS [Problema]
FROM [facturacion].[DocumentoElectronico] e
WHERE e.[Estado] IN ('Validado', 'Entregado') AND (e.[Cufe] IS NULL OR LTRIM(RTRIM(e.[Cufe])) = '');

SELECT CAST(e.[Id] AS nvarchar(50)) + N': PDF generado pero electronico aun en borrador' AS [Problema]
FROM [facturacion].[DocumentoElectronico] e
WHERE e.[PdfGeneradoEnUtc] IS NOT NULL AND e.[Estado] = 'Borrador';
