IF OBJECT_ID(N'contabilidad.MapeoRolContable', 'U') IS NULL
    THROW 50002, 'Falta contabilidad.MapeoRolContable.', 1;

IF COL_LENGTH(N'contabilidad.SolicitudContabilizacionIntegracion', N'ComprobanteId') IS NULL
    THROW 50002, 'Falta columna ComprobanteId en SolicitudContabilizacionIntegracion.', 1;
