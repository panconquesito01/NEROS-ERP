/*
===============================================================================
Neros ERP
Script        : V0005__mapeo_roles_comprobante_integracion.sql
Modulo        : contabilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Mapeo rol→cuenta y enlace solicitud→comprobante (cierre D-03).
Dependencias  : V0004__solicitudes_integracion.sql
Objetos       : contabilidad.MapeoRolContable, columna ComprobanteId en solicitud
Motivo        : Contabilizacion automatica desde integracion.
Impacto       : Tabla nueva y columna nullable.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/005_mapeo_roles_comprobante.sql
===============================================================================
*/

ALTER TABLE [contabilidad].[SolicitudContabilizacionIntegracion]
    ADD [ComprobanteId] uniqueidentifier NULL;
GO

ALTER TABLE [contabilidad].[SolicitudContabilizacionIntegracion]
    ADD CONSTRAINT [FK_SolicitudContab_Comprobante]
        FOREIGN KEY ([ComprobanteId]) REFERENCES [contabilidad].[Comprobante] ([Id]);
GO

CREATE TABLE [contabilidad].[MapeoRolContable]
(
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Rol] varchar(60) NOT NULL,
    [CuentaContableId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_MapeoRolContable] PRIMARY KEY ([TenantId], [EmpresaId], [Rol]),
    CONSTRAINT [FK_MapeoRol_Cuenta] FOREIGN KEY ([CuentaContableId]) REFERENCES [contabilidad].[CuentaContable] ([Id])
);
GO
