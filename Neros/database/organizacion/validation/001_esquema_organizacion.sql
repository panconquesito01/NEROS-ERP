/*
Validacion post-despliegue del modulo organizacion.
*/

IF SCHEMA_ID(N'organizacion') IS NULL
    THROW 50001, 'Falta el esquema organizacion.', 1;

IF OBJECT_ID(N'organizacion.Tenant', 'U') IS NULL
    THROW 50002, 'Falta la tabla organizacion.Tenant.', 1;

IF OBJECT_ID(N'organizacion.Empresa', 'U') IS NULL
    THROW 50003, 'Falta la tabla organizacion.Empresa.', 1;

IF OBJECT_ID(N'organizacion.Sucursal', 'U') IS NULL
    THROW 50004, 'Falta la tabla organizacion.Sucursal.', 1;

IF NOT EXISTS (SELECT 1 FROM [organizacion].[Tenant] WHERE [Codigo] = 'NEROS-TEST')
    THROW 50005, 'Falta el tenant NEROS-TEST en semilla.', 1;
