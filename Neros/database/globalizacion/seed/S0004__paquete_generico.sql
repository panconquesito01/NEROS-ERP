/*
===============================================================================
Neros ERP
Script        : S0004__paquete_generico.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Paquete de localizacion generico (sin reglas de pais).
Dependencias  : S0001__catalogo_iso.sql
Objetos       : globalizacion.PaqueteLocalizacion
Motivo        : Plan §30 paquete generico.
Impacto       : Dato de referencia.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

IF NOT EXISTS (SELECT 1 FROM [globalizacion].[PaqueteLocalizacion] WHERE [Codigo] = 'GENERICO')
    INSERT INTO [globalizacion].[PaqueteLocalizacion] ([Codigo], [Pais], [Nombre], [Version])
    VALUES ('GENERICO', NULL, N'Paquete generico Neros', '1.0.0');
GO
