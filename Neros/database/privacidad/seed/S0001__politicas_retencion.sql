/*
===============================================================================
Neros ERP
Script        : S0001__politicas_retencion.sql
Modulo        : privacidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Politicas de retencion iniciales para auditoria, sesiones y
                aceptaciones legales.
Dependencias  : V0001__esquema_privacidad.sql
Objetos       : privacidad.RetencionPolicy
Motivo        : Plan maestro §27.
Impacto       : Catalogo de referencia; parametrizable por jurisdiccion.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

MERGE [privacidad].[RetencionPolicy] AS destino
USING (VALUES
    ('AUDITORIA_EVENTO', N'Eventos de auditoria de seguridad', 'GLOBAL', 1825, N'5 anos', N'Fecha del evento',
        N'Integridad y trazabilidad operativa; revisar con asesor legal antes de produccion.', 'Conservar', 1),
    ('SESION_ACCESO', N'Sesiones de acceso revocadas o vencidas', 'GLOBAL', 90, N'90 dias', N'Revocacion o vencimiento',
        N'Operacion y seguridad; no sustituye obligaciones contables o legales.', 'Eliminar', 1),
    ('ACEPTACION_LEGAL', N'Registro de aceptacion de documentos legales', 'GLOBAL', NULL, N'Mientras dure la relacion y plazos legales', N'Fecha de aceptacion',
        N'Evidencia de consentimiento o aceptacion contractual.', 'Conservar', 1),
    ('CUENTA_USUARIO', N'Datos de cuenta de usuario activa', 'GLOBAL', NULL, N'Mientras la cuenta este activa mas plazos legales', N'Ultimo acceso o baja',
        N'Prestacion del servicio y soporte.', 'Conservar', 1)
) AS origen ([Codigo], [TipoInformacion], [Jurisdiccion], [PeriodoDias], [PeriodoDescripcion], [InicioComputo], [Fundamento], [AccionFinal], [Activo])
ON destino.[Codigo] = origen.[Codigo]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Codigo], [TipoInformacion], [Jurisdiccion], [PeriodoDias], [PeriodoDescripcion], [InicioComputo], [Fundamento], [AccionFinal], [Activo])
    VALUES (origen.[Codigo], origen.[TipoInformacion], origen.[Jurisdiccion], origen.[PeriodoDias], origen.[PeriodoDescripcion],
        origen.[InicioComputo], origen.[Fundamento], origen.[AccionFinal], origen.[Activo]);
GO
