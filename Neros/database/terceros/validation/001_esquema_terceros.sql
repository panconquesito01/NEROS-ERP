/*
Validacion post-despliegue del modulo terceros.
*/

IF SCHEMA_ID(N'terceros') IS NULL
    THROW 50001, 'Falta el esquema terceros.', 1;

IF OBJECT_ID(N'terceros.Tercero', 'U') IS NULL
    THROW 50002, 'Falta la tabla terceros.Tercero.', 1;

IF OBJECT_ID(N'terceros.TerceroHistorial', 'U') IS NULL
    THROW 50003, 'Falta el historial temporal terceros.TerceroHistorial.', 1;

IF OBJECT_ID(N'terceros.Identificacion', 'U') IS NULL
    THROW 50004, 'Falta la tabla terceros.Identificacion.', 1;

IF OBJECT_ID(N'terceros.Rol', 'U') IS NULL
    THROW 50005, 'Falta la tabla terceros.Rol.', 1;
