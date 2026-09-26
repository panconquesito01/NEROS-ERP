/*
Validacion outbox/inbox del modulo analitica.
*/

IF SCHEMA_ID(N'integracion') IS NULL
    THROW 50025, 'Falta el esquema integracion.', 1;

IF OBJECT_ID(N'integracion.MensajeSalida', N'U') IS NULL
    THROW 50025, 'Falta integracion.MensajeSalida.', 1;

IF OBJECT_ID(N'integracion.MensajeEntrada', N'U') IS NULL
    THROW 50025, 'Falta integracion.MensajeEntrada.', 1;
