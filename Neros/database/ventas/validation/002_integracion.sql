IF OBJECT_ID(N'integracion.MensajeSalida', 'U') IS NULL
    THROW 50010, 'Falta integracion.MensajeSalida.', 1;

IF OBJECT_ID(N'integracion.MensajeEntrada', 'U') IS NULL
    THROW 50011, 'Falta integracion.MensajeEntrada.', 1;
