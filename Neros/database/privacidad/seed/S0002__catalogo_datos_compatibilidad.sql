/*
===============================================================================
Neros ERP
Script        : S0002__catalogo_datos_compatibilidad.sql
Modulo        : privacidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Inventario inicial de datos personales del modulo compatibilidad.
Dependencias  : V0001__esquema_privacidad.sql, S0001__politicas_retencion.sql
Objetos       : privacidad.CatalogoDatoPersonal
Motivo        : Plan maestro §22.
Impacto       : Catalogo de referencia para revision de codigo.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

MERGE [privacidad].[CatalogoDatoPersonal] AS destino
USING (VALUES
    ('AspNetUsers.Email', 'compatibilidad', 'Personal', N'Identificar la cuenta y permitir el acceso.', N'Proporcionado por el titular o administrador.', 'CUENTA_USUARIO', NULL),
    ('AspNetUsers.Nombre', 'compatibilidad', 'Personal', N'Mostrar identidad en la interfaz y auditoria.', N'Proporcionado por el titular o administrador.', 'CUENTA_USUARIO', NULL),
    ('Sesiones.Ip', 'compatibilidad', 'Personal', N'Seguridad de sesiones y deteccion de accesos anomalos.', N'Generado en el acceso (BFF/API).', 'SESION_ACCESO', NULL),
    ('Sesiones.AgenteUsuario', 'compatibilidad', 'Personal', N'Descripcion aproximada del dispositivo en Mis sesiones.', N'Cabecera User-Agent del navegador.', 'SESION_ACCESO', NULL),
    ('auditoria.Evento.Ip', 'compatibilidad', 'Personal', N'Trazabilidad de acciones sensibles.', N'Generado en la operacion.', 'AUDITORIA_EVENTO', NULL),
    ('auditoria.Evento.AgenteUsuario', 'compatibilidad', 'Personal', N'Contexto del cliente en auditoria.', N'Cabecera User-Agent.', 'AUDITORIA_EVENTO', NULL),
    ('auditoria.Evento.ActorId', 'compatibilidad', 'Restringido', N'Identificar quien realizo la accion auditada.', N'Sistema de autenticacion.', 'AUDITORIA_EVENTO', NULL)
) AS origen ([Campo], [Modulo], [Clasificacion], [Finalidad], [Origen], [RetencionCodigo], [PermisoRequerido])
ON destino.[Campo] = origen.[Campo] AND destino.[Modulo] = origen.[Modulo]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Campo], [Modulo], [Clasificacion], [Finalidad], [Origen], [RetencionCodigo], [PermisoRequerido])
    VALUES (origen.[Campo], origen.[Modulo], origen.[Clasificacion], origen.[Finalidad], origen.[Origen], origen.[RetencionCodigo], origen.[PermisoRequerido]);
GO
