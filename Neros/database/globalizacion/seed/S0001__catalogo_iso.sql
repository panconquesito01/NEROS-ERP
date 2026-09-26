/*
===============================================================================
Neros ERP
Script        : S0001__catalogo_iso.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Paises y monedas ISO iniciales (paquete generico).
Dependencias  : V0001__esquema_globalizacion.sql
Objetos       : globalizacion.Pais, globalizacion.Moneda
Motivo        : Fase 6 catalogos ISO.
Impacto       : Datos de referencia.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

MERGE [globalizacion].[Pais] AS destino
USING (VALUES
    ('CO', N'Colombia'),
    ('US', N'Estados Unidos'),
    ('BR', N'Brasil'),
    ('MX', N'Mexico'),
    ('ES', N'Espana'),
    ('PE', N'Peru'),
    ('CL', N'Chile')
) AS origen ([Codigo], [Nombre])
ON destino.[Codigo] = origen.[Codigo]
WHEN NOT MATCHED THEN INSERT ([Codigo], [Nombre]) VALUES (origen.[Codigo], origen.[Nombre]);
GO

MERGE [globalizacion].[Moneda] AS destino
USING (VALUES
    ('COP', N'Peso colombiano', 2),
    ('USD', N'Dolar estadounidense', 2),
    ('EUR', N'Euro', 2),
    ('BRL', N'Real brasileno', 2),
    ('MXN', N'Peso mexicano', 2),
    ('JPY', N'Yen japones', 0),
    ('CLP', N'Peso chileno', 0),
    ('PEN', N'Sol peruano', 2)
) AS origen ([Codigo], [Nombre], [DecimalesIso4217])
ON destino.[Codigo] = origen.[Codigo]
WHEN NOT MATCHED THEN INSERT ([Codigo], [Nombre], [DecimalesIso4217])
    VALUES (origen.[Codigo], origen.[Nombre], origen.[DecimalesIso4217]);
GO
