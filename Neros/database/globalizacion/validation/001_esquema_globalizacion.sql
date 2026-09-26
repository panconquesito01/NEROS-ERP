SET NOCOUNT ON;

IF SCHEMA_ID(N'globalizacion') IS NULL THROW 50001, 'Falta esquema globalizacion.', 1;
IF SCHEMA_ID(N'cumplimiento') IS NULL THROW 50002, 'Falta esquema cumplimiento.', 1;

IF OBJECT_ID(N'globalizacion.Pais', N'U') IS NULL THROW 50003, 'Falta tabla globalizacion.Pais.', 1;
IF OBJECT_ID(N'globalizacion.Moneda', N'U') IS NULL THROW 50004, 'Falta tabla globalizacion.Moneda.', 1;
IF OBJECT_ID(N'globalizacion.PoliticaRedondeo', N'U') IS NULL THROW 50005, 'Falta tabla globalizacion.PoliticaRedondeo.', 1;
IF OBJECT_ID(N'globalizacion.TasaCambio', N'U') IS NULL THROW 50006, 'Falta tabla globalizacion.TasaCambio.', 1;
IF OBJECT_ID(N'cumplimiento.ReglaNormativa', N'U') IS NULL THROW 50007, 'Falta tabla cumplimiento.ReglaNormativa.', 1;

IF (SELECT COUNT(*) FROM [globalizacion].[Moneda]) < 4 THROW 50008, 'Catalogo de monedas incompleto.', 1;
IF (SELECT COUNT(*) FROM [globalizacion].[PoliticaRedondeo]) < 4 THROW 50009, 'Politicas de redondeo incompletas.', 1;
IF NOT EXISTS (SELECT 1 FROM [globalizacion].[PaqueteLocalizacion] WHERE [Codigo] = 'GENERICO')
    THROW 50010, 'Falta paquete GENERICO.', 1;

PRINT 'Validacion globalizacion OK';
