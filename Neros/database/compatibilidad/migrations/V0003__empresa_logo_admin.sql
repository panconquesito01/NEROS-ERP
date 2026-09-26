/*
===============================================================================
Neros ERP
Script        : V0003__empresa_logo_admin.sql
Modulo        : compatibilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Logo de empresa para administracion SaaS (D-07).
Dependencias  : V0001__identity_empresas.sql
Objetos       : columnas ImagenLogo en dbo.Empresas
Motivo        : Superusuario configura imagen por empresa.
Impacto       : Columnas nullable.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/003_empresa_logo.sql
===============================================================================
*/

ALTER TABLE [dbo].[Empresas]
    ADD [ImagenLogo] varbinary(max) NULL,
        [ImagenLogoContentType] varchar(100) NULL;
GO
