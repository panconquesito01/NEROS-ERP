IF OBJECT_ID(N'contabilidad.DefinicionReporte', 'U') IS NULL
    THROW 50020, 'Falta contabilidad.DefinicionReporte.', 1;

IF OBJECT_ID(N'contabilidad.MarcoContableEmpresa', 'U') IS NULL
    THROW 50021, 'Falta contabilidad.MarcoContableEmpresa.', 1;

SELECT CAST(l.[Id] AS nvarchar(50)) + N': linea formula sin tipo' AS [Problema]
FROM [contabilidad].[LineaReporte] l
WHERE l.[TipoLinea] = 'Formula' AND l.[TipoFormula] IS NULL;
