/*
===============================================================================
Neros ERP
Script        : S0001__jurisdiccion_co.sql
Modulo        : impuestos
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Jurisdiccion Colombia de referencia.
Dependencias  : V0001__esquema_impuestos.sql
Objetos       : impuestos.Jurisdiccion, ConceptoTributario
Motivo        : Plan fase 8.
Impacto       : Datos de referencia.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

INSERT INTO [impuestos].[Jurisdiccion] ([Codigo], [Nombre], [Pais])
SELECT 'CO-IVA', N'IVA Colombia', 'CO'
WHERE NOT EXISTS (SELECT 1 FROM [impuestos].[Jurisdiccion] WHERE [Codigo] = 'CO-IVA');
GO

INSERT INTO [impuestos].[ConceptoTributario] ([Codigo], [Nombre], [JurisdiccionCodigo])
SELECT 'BIENES', N'Bienes gravados', 'CO-IVA'
WHERE NOT EXISTS (SELECT 1 FROM [impuestos].[ConceptoTributario] WHERE [Codigo] = 'BIENES');
GO
