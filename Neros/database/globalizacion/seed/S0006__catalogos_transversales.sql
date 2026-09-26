/*
===============================================================================
Neros ERP
Script        : S0006__catalogos_transversales.sql
Modulo        : globalizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Zonas horarias, unidades base y tipos de identificacion iniciales.
Dependencias  : V0002__catalogos_transversales.sql, S0001__catalogo_iso.sql
Objetos       : ZonaHoraria, UnidadMedida, TipoIdentificacion
Motivo        : Catalogos transversales a todas las empresas (fase 6, §30).
Impacto       : Datos de referencia idempotentes.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

MERGE [globalizacion].[ZonaHoraria] AS destino
USING (VALUES
    (N'America/Bogota', N'Bogota (Colombia)'),
    (N'America/Mexico_City', N'Ciudad de Mexico'),
    (N'America/Sao_Paulo', N'Sao Paulo'),
    (N'America/Lima', N'Lima'),
    (N'America/Santiago', N'Santiago'),
    (N'Europe/Madrid', N'Madrid'),
    (N'UTC', N'UTC')
) AS origen ([Id], [Nombre])
ON destino.[Id] = origen.[Id]
WHEN NOT MATCHED THEN INSERT ([Id], [Nombre]) VALUES (origen.[Id], origen.[Nombre]);
GO

MERGE [globalizacion].[UnidadMedida] AS destino
USING (VALUES
    (N'UND', N'Unidad', N'und'),
    (N'KG', N'Kilogramo', N'kg'),
    (N'GR', N'Gramo', N'g'),
    (N'LT', N'Litro', N'L'),
    (N'MT', N'Metro', N'm'),
    (N'HR', N'Hora', N'h'),
    (N'SRV', N'Servicio', N'srv')
) AS origen ([Codigo], [Nombre], [Simbolo])
ON destino.[Codigo] = origen.[Codigo]
WHEN NOT MATCHED THEN INSERT ([Codigo], [Nombre], [Simbolo]) VALUES (origen.[Codigo], origen.[Nombre], origen.[Simbolo]);
GO

MERGE [globalizacion].[TipoIdentificacion] AS destino
USING (VALUES
    (N'CO', N'NIT', N'NIT'),
    (N'CO', N'CC', N'Cedula de ciudadania'),
    (N'CO', N'CE', N'Cedula de extranjeria'),
    (N'CO', N'PA', N'Pasaporte'),
    (N'US', N'EIN', N'Employer Identification Number'),
    (N'US', N'SSN', N'Social Security Number'),
    (N'MX', N'RFC', N'RFC'),
    (N'BR', N'CNPJ', N'CNPJ')
) AS origen ([Pais], [Codigo], [Nombre])
ON destino.[Pais] = origen.[Pais] AND destino.[Codigo] = origen.[Codigo]
WHEN NOT MATCHED THEN INSERT ([Pais], [Codigo], [Nombre]) VALUES (origen.[Pais], origen.[Codigo], origen.[Nombre]);
GO
