/*
===============================================================================
Neros ERP
Script        : V0003__reportes_y_diferencia_cambio.sql
Modulo        : contabilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Reporting configurable y marco contable (plan fase 16, §41, §45).
Dependencias  : V0002__integracion_outbox_inbox.sql
Objetos       : MarcoContableEmpresa, DefinicionReporte, SeccionReporte, LineaReporte, MapeoCuentaReporte
Motivo        : Estados financieros e indicadores configurables.
Impacto       : Tablas nuevas de configuracion.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/003_reportes.sql
===============================================================================
*/

CREATE TABLE [contabilidad].[MarcoContableEmpresa]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MarcoContableEmpresa] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [CodigoMarco] varchar(20) NOT NULL,
    [VigenteDesde] date NOT NULL,
    [VigenteHasta] date NULL,
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Marco_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_MarcoContable_Codigo] CHECK ([CodigoMarco] IN ('CO-GRUPO1', 'CO-GRUPO2', 'CO-GRUPO3')),
    CONSTRAINT [CK_MarcoContable_Rango] CHECK ([VigenteHasta] IS NULL OR [VigenteHasta] >= [VigenteDesde])
);
GO

CREATE INDEX [IX_MarcoContable_Empresa]
    ON [contabilidad].[MarcoContableEmpresa] ([TenantId], [EmpresaId], [VigenteDesde] DESC);
GO

CREATE TABLE [contabilidad].[DefinicionReporte]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DefinicionReporte] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(40) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [TipoReporte] varchar(30) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_DefinicionReporte_Activo] DEFAULT (1),
    CONSTRAINT [CK_DefinicionReporte_Tipo] CHECK ([TipoReporte] IN (
        'SituacionFinanciera', 'EstadoResultados', 'FlujoEfectivo', 'CambiosPatrimonio'))
);
GO

CREATE UNIQUE INDEX [UX_DefinicionReporte_Empresa_Codigo]
    ON [contabilidad].[DefinicionReporte] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [contabilidad].[SeccionReporte]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_SeccionReporte] PRIMARY KEY,
    [DefinicionReporteId] uniqueidentifier NOT NULL,
    [Codigo] varchar(40) NOT NULL,
    [Etiqueta] nvarchar(120) NOT NULL,
    [Orden] int NOT NULL,
    CONSTRAINT [FK_SeccionReporte_Definicion] FOREIGN KEY ([DefinicionReporteId]) REFERENCES [contabilidad].[DefinicionReporte] ([Id])
);
GO

CREATE UNIQUE INDEX [UX_SeccionReporte_Orden]
    ON [contabilidad].[SeccionReporte] ([DefinicionReporteId], [Orden]);
GO

CREATE TABLE [contabilidad].[LineaReporte]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_LineaReporte] PRIMARY KEY,
    [SeccionReporteId] uniqueidentifier NOT NULL,
    [Codigo] varchar(40) NOT NULL,
    [Etiqueta] nvarchar(120) NOT NULL,
    [Orden] int NOT NULL,
    [TipoLinea] varchar(20) NOT NULL,
    [TipoFormula] varchar(20) NULL,
    [Operando1CodigoLinea] varchar(40) NULL,
    [Operando2CodigoLinea] varchar(40) NULL,
    CONSTRAINT [FK_LineaReporte_Seccion] FOREIGN KEY ([SeccionReporteId]) REFERENCES [contabilidad].[SeccionReporte] ([Id]),
    CONSTRAINT [CK_LineaReporte_TipoLinea] CHECK ([TipoLinea] IN ('Cuenta', 'Formula', 'Subtotal')),
    CONSTRAINT [CK_LineaReporte_Formula] CHECK ([TipoFormula] IS NULL OR [TipoFormula] IN (
        'SUM', 'SUBTRACT', 'PERCENT', 'VARIATION', 'RATIO'))
);
GO

CREATE UNIQUE INDEX [UX_LineaReporte_Orden]
    ON [contabilidad].[LineaReporte] ([SeccionReporteId], [Orden]);
GO

CREATE TABLE [contabilidad].[MapeoCuentaReporte]
(
    [LineaReporteId] uniqueidentifier NOT NULL,
    [CuentaContableId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_MapeoCuentaReporte] PRIMARY KEY ([LineaReporteId], [CuentaContableId]),
    CONSTRAINT [FK_MapeoCuenta_Linea] FOREIGN KEY ([LineaReporteId]) REFERENCES [contabilidad].[LineaReporte] ([Id]),
    CONSTRAINT [FK_MapeoCuenta_Cuenta] FOREIGN KEY ([CuentaContableId]) REFERENCES [contabilidad].[CuentaContable] ([Id])
);
GO
