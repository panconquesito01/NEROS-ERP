/*
===============================================================================
Neros ERP
Script        : S0003__tasas_ejemplo.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Tasas de cambio de desarrollo (no productivas).
Dependencias  : S0001__catalogo_iso.sql
Objetos       : globalizacion.TasaCambio
Motivo        : Pruebas de conversion §31.
Impacto       : Datos de desarrollo.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

DECLARE @Fecha date = '2026-01-15';
DECLARE @Fuente varchar(40) = 'NEROS-DEV';

IF NOT EXISTS (SELECT 1 FROM [globalizacion].[TasaCambio] WHERE [Fuente] = @Fuente AND [Fecha] = @Fecha AND [MonedaOrigen] = 'USD' AND [MonedaDestino] = 'COP')
    INSERT INTO [globalizacion].[TasaCambio] ([Fuente], [Fecha], [MonedaOrigen], [MonedaDestino], [Valor])
    VALUES (@Fuente, @Fecha, 'USD', 'COP', 4200.000000000000);

IF NOT EXISTS (SELECT 1 FROM [globalizacion].[TasaCambio] WHERE [Fuente] = @Fuente AND [Fecha] = @Fecha AND [MonedaOrigen] = 'EUR' AND [MonedaDestino] = 'COP')
    INSERT INTO [globalizacion].[TasaCambio] ([Fuente], [Fecha], [MonedaOrigen], [MonedaDestino], [Valor])
    VALUES (@Fuente, @Fecha, 'EUR', 'COP', 4600.000000000000);
GO
