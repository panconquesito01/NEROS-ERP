/*
===============================================================================
Neros ERP
Script        : S0004__documentos_legales_base.sql
Modulo        : privacidad
Fecha         : 2026-09-25
Autor         : Equipo Neros
Descripcion   : Terminos y politica de privacidad v1 (texto operativo).
Dependencias  : V0001__esquema_privacidad.sql
Objetos       : privacidad.DocumentoLegal, DocumentoLegalVersion
Motivo        : Plan maestro §23; aceptacion registrada en fase 3.
Impacto       : Requiere aceptacion explicita antes de usar el sistema.
Destructivo   : NO
Transaccional : SI
Riesgo        : MEDIO
Rollback      : Roll forward; nueva version para cambios juridicos.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

DECLARE @TerminosId uniqueidentifier = 'a1a1a1a1-1111-4111-8111-111111111101';
DECLARE @PrivacidadId uniqueidentifier = 'a1a1a1a1-1111-4111-8111-111111111102';
DECLARE @ContenidoTerminos nvarchar(max) = N'# Terminos de uso (borrador operativo)

Este texto es un **marcador de posicion** para desarrollo y pruebas. Debe ser sustituido o aprobado por el asesor legal del operador antes de produccion.

Al usar Neros ERP aceptas utilizar la plataforma conforme a las politicas internas de tu organizacion y a las funciones disponibles en la version actual.

## Servicio

Neros ERP es software en evolucion. Las funciones descritas en la documentacion pueden cambiar entre versiones.

## Cuenta

Eres responsable de la confidencialidad de tus credenciales y de cerrar sesion en equipos compartidos.

## Contacto

Para condiciones comerciales o juridicas definitivas, contacta al administrador de tu despliegue.';
DECLARE @ContenidoPrivacidad nvarchar(max) = N'# Politica de privacidad (borrador operativo)

Texto provisional para desarrollo. Requiere revision juridica por pais y rol (responsable/encargado) antes de produccion.

## Datos que tratamos hoy

- Datos de cuenta (correo, nombre).
- Datos de sesion (IP, agente de usuario, fechas).
- Registros de auditoria de seguridad.

Consulta el inventario tecnico en el modulo de privacidad y la pagina /cookies.

## Finalidad

Autenticacion, seguridad, soporte operativo y cumplimiento de obligaciones aplicables.

## Derechos

Los titulares pueden ejercer derechos mediante los canales que habilite el operador. Las solicitudes pueden quedar sujetas a retencion legal o contable.

## Conservacion

Aplican las politicas de retencion configuradas (auditoria, sesiones, aceptaciones).';

IF NOT EXISTS (SELECT 1 FROM [privacidad].[DocumentoLegal] WHERE [Codigo] = 'terminos')
    INSERT INTO [privacidad].[DocumentoLegal] ([Id], [Codigo], [Nombre], [RequiereAceptacion], [Orden], [Activo])
    VALUES (@TerminosId, 'terminos', N'Terminos de uso', 1, 10, 1);

IF NOT EXISTS (SELECT 1 FROM [privacidad].[DocumentoLegal] WHERE [Codigo] = 'privacidad')
    INSERT INTO [privacidad].[DocumentoLegal] ([Id], [Codigo], [Nombre], [RequiereAceptacion], [Orden], [Activo])
    VALUES (@PrivacidadId, 'privacidad', N'Politica de privacidad', 1, 20, 1);

IF NOT EXISTS (SELECT 1 FROM [privacidad].[DocumentoLegalVersion] AS v
    INNER JOIN [privacidad].[DocumentoLegal] AS d ON d.[Id] = v.[DocumentoId] WHERE d.[Codigo] = 'terminos' AND v.[Version] = 1)
    INSERT INTO [privacidad].[DocumentoLegalVersion] ([Id], [DocumentoId], [Version], [Contenido], [HashContenido], [VigenteDesde], [PublicadoPor])
    SELECT NEWID(), @TerminosId, 1, @ContenidoTerminos,
        LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', @ContenidoTerminos), 2)),
        SYSUTCDATETIME(), SUSER_SNAME();

IF NOT EXISTS (SELECT 1 FROM [privacidad].[DocumentoLegalVersion] AS v
    INNER JOIN [privacidad].[DocumentoLegal] AS d ON d.[Id] = v.[DocumentoId] WHERE d.[Codigo] = 'privacidad' AND v.[Version] = 1)
    INSERT INTO [privacidad].[DocumentoLegalVersion] ([Id], [DocumentoId], [Version], [Contenido], [HashContenido], [VigenteDesde], [PublicadoPor])
    SELECT NEWID(), @PrivacidadId, 1, @ContenidoPrivacidad,
        LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', @ContenidoPrivacidad), 2)),
        SYSUTCDATETIME(), SUSER_SNAME();
GO
