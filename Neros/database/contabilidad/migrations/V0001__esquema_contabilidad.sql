/*
===============================================================================
Neros ERP
Script        : V0001__esquema_contabilidad.sql
Modulo        : contabilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Plan de cuentas, periodos, comprobantes y movimientos (plan fase 9, §33–§35).
Dependencias  : Ninguna
Objetos       : contabilidad.Ejercicio, Periodo, CuentaContable, TipoComprobante,
                Comprobante, MovimientoContable, DocumentoOrigen
Motivo        : Base del modulo Contabilidad.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_contabilidad.sql
===============================================================================
*/

CREATE SCHEMA [contabilidad];
GO

CREATE TABLE [contabilidad].[Ejercicio]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Ejercicio] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Anio] int NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Ejercicio_Activo] DEFAULT (1),
    CONSTRAINT [CK_Ejercicio_Anio] CHECK ([Anio] BETWEEN 1900 AND 2100)
);
GO

CREATE UNIQUE INDEX [UX_Ejercicio_Empresa_Anio]
    ON [contabilidad].[Ejercicio] ([TenantId], [EmpresaId], [Anio]);
GO

CREATE TABLE [contabilidad].[Periodo]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Periodo] PRIMARY KEY,
    [EjercicioId] uniqueidentifier NOT NULL,
    [Numero] int NOT NULL,
    [FechaInicio] date NOT NULL,
    [FechaFin] date NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Periodo_Estado] DEFAULT ('Abierto'),
    [EsAjusteCierre] bit NOT NULL CONSTRAINT [DF_Periodo_Ajuste] DEFAULT (0),
    CONSTRAINT [FK_Periodo_Ejercicio] FOREIGN KEY ([EjercicioId]) REFERENCES [contabilidad].[Ejercicio] ([Id]),
    CONSTRAINT [CK_Periodo_Estado] CHECK ([Estado] IN ('Abierto', 'EnCierre', 'Cerrado', 'Reabierto')),
    CONSTRAINT [CK_Periodo_Rango] CHECK ([FechaFin] >= [FechaInicio])
);
GO

CREATE UNIQUE INDEX [UX_Periodo_Ejercicio_Numero]
    ON [contabilidad].[Periodo] ([EjercicioId], [Numero]);
GO

CREATE TABLE [contabilidad].[CuentaContable]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_CuentaContable] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(20) NOT NULL,
    [Nombre] nvarchar(160) NOT NULL,
    [Naturaleza] char(1) NOT NULL,
    [Tipo] varchar(20) NOT NULL,
    [Nivel] tinyint NOT NULL,
    [AdmiteMovimiento] bit NOT NULL CONSTRAINT [DF_Cuenta_Admite] DEFAULT (1),
    [ExigeTercero] bit NOT NULL CONSTRAINT [DF_Cuenta_Tercero] DEFAULT (0),
    [ExigeCentroCosto] bit NOT NULL CONSTRAINT [DF_Cuenta_Centro] DEFAULT (0),
    [Activa] bit NOT NULL CONSTRAINT [DF_Cuenta_Activa] DEFAULT (1),
    CONSTRAINT [CK_Cuenta_Naturaleza] CHECK ([Naturaleza] IN ('D', 'C')),
    CONSTRAINT [CK_Cuenta_Tipo] CHECK ([Tipo] IN ('Activo', 'Pasivo', 'Patrimonio', 'Ingreso', 'Costo', 'Gasto', 'Orden'))
);
GO

CREATE UNIQUE INDEX [UX_Cuenta_Empresa_Codigo]
    ON [contabilidad].[CuentaContable] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [contabilidad].[TipoComprobante]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_TipoComprobante] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(10) NOT NULL,
    [Nombre] nvarchar(80) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_TipoComprobante_Activo] DEFAULT (1)
);
GO

CREATE UNIQUE INDEX [UX_TipoComprobante_Empresa_Codigo]
    ON [contabilidad].[TipoComprobante] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [contabilidad].[Comprobante]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Comprobante] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [PeriodoId] uniqueidentifier NOT NULL,
    [TipoComprobanteId] uniqueidentifier NOT NULL,
    [Numero] int NOT NULL,
    [Fecha] date NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Comprobante_Estado] DEFAULT ('Borrador'),
    [ComprobanteOriginalId] uniqueidentifier NULL,
    [ComprobanteReversionId] uniqueidentifier NULL,
    [MotivoReversion] nvarchar(500) NULL,
    [ModuloOrigen] varchar(40) NULL,
    [TipoDocumentoOrigen] varchar(40) NULL,
    [DocumentoOrigenId] uniqueidentifier NULL,
    [NumeroDocumentoOrigen] varchar(40) NULL,
    [ReglaContabilizacionVersion] int NULL,
    CONSTRAINT [FK_Comprobante_Periodo] FOREIGN KEY ([PeriodoId]) REFERENCES [contabilidad].[Periodo] ([Id]),
    CONSTRAINT [FK_Comprobante_Tipo] FOREIGN KEY ([TipoComprobanteId]) REFERENCES [contabilidad].[TipoComprobante] ([Id]),
    CONSTRAINT [FK_Comprobante_Original] FOREIGN KEY ([ComprobanteOriginalId]) REFERENCES [contabilidad].[Comprobante] ([Id]),
    CONSTRAINT [FK_Comprobante_Reversion] FOREIGN KEY ([ComprobanteReversionId]) REFERENCES [contabilidad].[Comprobante] ([Id]),
    CONSTRAINT [CK_Comprobante_Estado] CHECK ([Estado] IN ('Borrador', 'Contabilizado', 'Reversado'))
);
GO

CREATE UNIQUE INDEX [UX_Comprobante_Empresa_Tipo_Numero]
    ON [contabilidad].[Comprobante] ([EmpresaId], [TipoComprobanteId], [Numero]);
GO

CREATE TABLE [contabilidad].[MovimientoContable]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MovimientoContable] PRIMARY KEY DEFAULT (NEWSEQUENTIALID()),
    [ComprobanteId] uniqueidentifier NOT NULL,
    [Linea] int NOT NULL,
    [CuentaContableId] uniqueidentifier NOT NULL,
    [Debito] decimal(19, 4) NOT NULL CONSTRAINT [DF_Movimiento_Debito] DEFAULT (0),
    [Credito] decimal(19, 4) NOT NULL CONSTRAINT [DF_Movimiento_Credito] DEFAULT (0),
    [ImporteMonedaDocumento] decimal(19, 4) NOT NULL,
    [MonedaDocumento] char(3) NOT NULL,
    [TasaCambio] decimal(19, 8) NOT NULL CONSTRAINT [DF_Movimiento_Tasa] DEFAULT (1),
    [ImporteMonedaFuncional] decimal(19, 4) NOT NULL,
    [TerceroId] uniqueidentifier NULL,
    [CentroCostoId] uniqueidentifier NULL,
    CONSTRAINT [FK_Movimiento_Comprobante] FOREIGN KEY ([ComprobanteId]) REFERENCES [contabilidad].[Comprobante] ([Id]),
    CONSTRAINT [FK_Movimiento_Cuenta] FOREIGN KEY ([CuentaContableId]) REFERENCES [contabilidad].[CuentaContable] ([Id]),
    CONSTRAINT [CK_Movimiento_Lado] CHECK (
        ([Debito] >= 0 AND [Credito] >= 0)
        AND (([Debito] > 0 AND [Credito] = 0) OR ([Credito] > 0 AND [Debito] = 0) OR ([Debito] = 0 AND [Credito] = 0))),
    CONSTRAINT [CK_Movimiento_Funcional] CHECK ([ImporteMonedaFuncional] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_Movimiento_Comprobante_Linea]
    ON [contabilidad].[MovimientoContable] ([ComprobanteId], [Linea]);
GO

CREATE TABLE [contabilidad].[DocumentoOrigen]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_DocumentoOrigen] PRIMARY KEY,
    [ComprobanteId] uniqueidentifier NOT NULL,
    [ModuloOrigen] varchar(40) NOT NULL,
    [TipoDocumentoOrigen] varchar(40) NOT NULL,
    [DocumentoOrigenId] uniqueidentifier NOT NULL,
    [NumeroDocumentoOrigen] varchar(40) NOT NULL,
    CONSTRAINT [FK_DocumentoOrigen_Comprobante] FOREIGN KEY ([ComprobanteId]) REFERENCES [contabilidad].[Comprobante] ([Id])
);
GO
