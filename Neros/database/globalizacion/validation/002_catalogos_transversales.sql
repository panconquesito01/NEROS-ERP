SET NOCOUNT ON;

IF OBJECT_ID(N'globalizacion.ZonaHoraria', N'U') IS NULL THROW 50011, 'Falta tabla globalizacion.ZonaHoraria.', 1;
IF OBJECT_ID(N'globalizacion.UnidadMedida', N'U') IS NULL THROW 50012, 'Falta tabla globalizacion.UnidadMedida.', 1;
IF OBJECT_ID(N'globalizacion.TipoIdentificacion', N'U') IS NULL THROW 50013, 'Falta tabla globalizacion.TipoIdentificacion.', 1;

IF (SELECT COUNT(*) FROM [globalizacion].[ZonaHoraria]) < 5 THROW 50014, 'Catalogo de zonas horarias incompleto.', 1;
IF (SELECT COUNT(*) FROM [globalizacion].[UnidadMedida]) < 5 THROW 50015, 'Catalogo de unidades incompleto.', 1;
IF NOT EXISTS (SELECT 1 FROM [globalizacion].[TipoIdentificacion] WHERE [Pais] = 'CO' AND [Codigo] = 'NIT')
    THROW 50016, 'Falta tipo NIT Colombia.', 1;

PRINT 'Validacion catalogos transversales OK';
