/*
Validacion post-despliegue del modulo impuestos.
*/

IF SCHEMA_ID(N'impuestos') IS NULL
    THROW 50001, 'Falta el esquema impuestos.', 1;

IF OBJECT_ID(N'impuestos.VersionPublicada', 'U') IS NULL
    THROW 50002, 'Falta la tabla impuestos.VersionPublicada.', 1;

IF OBJECT_ID(N'impuestos.VersionRegla', 'U') IS NULL
    THROW 50003, 'Falta la tabla impuestos.VersionRegla.', 1;

IF NOT EXISTS (SELECT 1 FROM [impuestos].[Jurisdiccion] WHERE [Codigo] = 'CO-IVA')
    THROW 50004, 'Falta la jurisdiccion CO-IVA en semilla.', 1;
