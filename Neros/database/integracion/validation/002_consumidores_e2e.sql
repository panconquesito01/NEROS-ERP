-- Validacion operativa fase 24: inbox/outbox presentes en modulos integrados.
-- Ejecutar con permisos de lectura sobre cada base de modulo.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'integracion')
    THROW 50024, 'Falta esquema integracion.', 1;

IF OBJECT_ID(N'integracion.MensajeSalida', N'U') IS NULL
    THROW 50024, 'Falta integracion.MensajeSalida.', 1;

IF OBJECT_ID(N'integracion.MensajeEntrada', N'U') IS NULL
    THROW 50024, 'Falta integracion.MensajeEntrada.', 1;

SELECT N'integracion_e2e_ok' AS Resultado;
