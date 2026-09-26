/*
===============================================================================
Neros ERP
Script        : S0001__tenant_prueba.sql
Modulo        : organizacion
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Tenant NEROS-TEST para desarrollo (sin empresas; se sincronizan en pruebas).
Dependencias  : V0001__esquema_organizacion.sql
Objetos       : organizacion.Tenant
Motivo        : Manifiesto deploy/development/company-tenant-map.json
Impacto       : Dato de desarrollo; no asignacion automatica en runtime.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

DECLARE @TenantId uniqueidentifier = 'f70c8608-2f4a-4d79-90b8-30f1b569bd17';

IF NOT EXISTS (SELECT 1 FROM [organizacion].[Tenant] WHERE [Id] = @TenantId)
    INSERT INTO [organizacion].[Tenant] ([Id], [Codigo], [Nombre], [Activo])
    VALUES (@TenantId, 'NEROS-TEST', N'Tenant de pruebas Neros', 1);
GO
