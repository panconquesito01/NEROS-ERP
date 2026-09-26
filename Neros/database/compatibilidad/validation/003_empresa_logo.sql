IF COL_LENGTH(N'dbo.Empresas', N'ImagenLogo') IS NULL
    THROW 50002, 'Falta columna ImagenLogo en Empresas.', 1;

IF COL_LENGTH(N'dbo.Empresas', N'ImagenLogoContentType') IS NULL
    THROW 50002, 'Falta columna ImagenLogoContentType en Empresas.', 1;
