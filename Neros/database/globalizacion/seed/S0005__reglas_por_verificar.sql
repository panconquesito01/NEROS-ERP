/*
===============================================================================
Neros ERP
Script        : S0005__reglas_por_verificar.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Registro normativo inicial en estado POR_VERIFICAR.
Dependencias  : S0001__catalogo_iso.sql
Objetos       : cumplimiento.ReglaNormativa
Motivo        : Plan §29.
Impacto       : Datos de referencia; no certifica cumplimiento.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

IF NOT EXISTS (SELECT 1 FROM [cumplimiento].[ReglaNormativa] WHERE [Pais] = 'CO' AND [Modulo] = 'Privacidad' AND [Codigo] = 'LEY1581')
    INSERT INTO [cumplimiento].[ReglaNormativa]
        ([Pais], [Jurisdiccion], [Modulo], [Codigo], [Nombre], [Descripcion], [Fuente], [Version], [Estado])
    VALUES
        ('CO', N'Nacional', 'Privacidad', 'LEY1581', N'Proteccion de datos personales',
         N'Referencia inicial a Ley 1581 de 2012; requiere revision de especialista.',
         N'https://www.funcionpublica.gov.co/eva/gestornormativo/norma.php?i=49981', '2026-01', 'POR_VERIFICAR');
GO
