/*
Validacion outbox/inbox del modulo presupuesto.
*/

IF SCHEMA_ID(N'integracion') IS NULL
    THROW 50010, 'Falta el esquema integracion.', 1;

IF OBJECT_ID(N'integracion.MensajeSalida', 'U') IS NULL
    THROW 50011, 'Falta integracion.MensajeSalida.', 1;

IF OBJECT_ID(N'integracion.MensajeEntrada', 'U') IS NULL
    THROW 50012, 'Falta integracion.MensajeEntrada.', 1;
