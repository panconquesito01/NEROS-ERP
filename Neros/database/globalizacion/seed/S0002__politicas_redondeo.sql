/*
===============================================================================
Neros ERP
Script        : S0002__politicas_redondeo.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Politicas de redondeo por defecto por moneda.
Dependencias  : S0001__catalogo_iso.sql
Objetos       : globalizacion.PoliticaRedondeo
Motivo        : Plan §32.
Impacto       : Datos de referencia.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

DECLARE @Tipo varchar(40) = 'DocumentoComercial';

INSERT INTO [globalizacion].[PoliticaRedondeo]
    ([Moneda], [TipoDocumento], [PrecisionCalculo], [PrecisionMoneda], [ModoRedondeo], [MomentoRedondeo])
SELECT m.[Codigo], @Tipo, 6, m.[DecimalesIso4217], 'AwayFromZero', 'PorLinea'
FROM [globalizacion].[Moneda] m
WHERE NOT EXISTS (
    SELECT 1 FROM [globalizacion].[PoliticaRedondeo] p
    WHERE p.[Moneda] = m.[Codigo] AND p.[TipoDocumento] = @Tipo);
GO
