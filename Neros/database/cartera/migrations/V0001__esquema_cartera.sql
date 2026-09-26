/*
===============================================================================
Neros ERP
Script        : V0001__esquema_cartera.sql
Modulo        : cartera
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : CxC, CxP, cuotas, movimientos inmutables y aplicaciones de pago (plan fase 14, §43).
Dependencias  : Ninguna
Objetos       : cartera.Documento, Cuota, Movimiento, Pago, AplicacionPago, ProyeccionSaldo
Motivo        : Base de cuentas por cobrar y por pagar.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_cartera.sql
===============================================================================
*/

CREATE SCHEMA [cartera];
GO

CREATE TABLE [cartera].[Documento]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Documento] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoCartera] varchar(3) NOT NULL,
    [TerceroId] uniqueidentifier NOT NULL,
    [Numero] varchar(40) NOT NULL,
    [FechaDocumento] date NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [TasaCambio] decimal(28, 10) NOT NULL CONSTRAINT [DF_Documento_Tasa] DEFAULT (1),
    [ModuloOrigen] varchar(30) NULL,
    [DocumentoOrigenId] uniqueidentifier NULL,
    [TotalDocumento] decimal(28, 10) NOT NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Documento_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_Documento_TipoCartera] CHECK ([TipoCartera] IN ('CxC', 'CxP')),
    CONSTRAINT [CK_Documento_Tasa] CHECK ([TasaCambio] > 0),
    CONSTRAINT [CK_Documento_Total] CHECK ([TotalDocumento] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_Documento_Empresa_Numero]
    ON [cartera].[Documento] ([TenantId], [EmpresaId], [TipoCartera], [Numero]);
GO

CREATE TABLE [cartera].[Cuota]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Cuota] PRIMARY KEY,
    [DocumentoId] uniqueidentifier NOT NULL,
    [NumeroCuota] int NOT NULL,
    [FechaVencimiento] date NOT NULL,
    [ImporteCuota] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_Cuota_Documento] FOREIGN KEY ([DocumentoId]) REFERENCES [cartera].[Documento] ([Id]),
    CONSTRAINT [CK_Cuota_Importe] CHECK ([ImporteCuota] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_Cuota_Documento_Numero]
    ON [cartera].[Cuota] ([DocumentoId], [NumeroCuota]);
GO

CREATE TABLE [cartera].[Movimiento]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Movimiento] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoCartera] varchar(3) NOT NULL,
    [TerceroId] uniqueidentifier NOT NULL,
    [DocumentoId] uniqueidentifier NULL,
    [CuotaId] uniqueidentifier NULL,
    [PagoId] uniqueidentifier NULL,
    [Naturaleza] varchar(10) NOT NULL,
    [Concepto] varchar(30) NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [FechaMovimiento] date NOT NULL,
    [Referencia] nvarchar(120) NULL,
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Movimiento_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_Movimiento_Documento] FOREIGN KEY ([DocumentoId]) REFERENCES [cartera].[Documento] ([Id]),
    CONSTRAINT [FK_Movimiento_Cuota] FOREIGN KEY ([CuotaId]) REFERENCES [cartera].[Cuota] ([Id]),
    CONSTRAINT [CK_Movimiento_TipoCartera] CHECK ([TipoCartera] IN ('CxC', 'CxP')),
    CONSTRAINT [CK_Movimiento_Naturaleza] CHECK ([Naturaleza] IN ('Cargo', 'Abono')),
    CONSTRAINT [CK_Movimiento_Importe] CHECK ([Importe] > 0),
    CONSTRAINT [CK_Movimiento_Concepto] CHECK ([Concepto] IN (
        'Factura', 'NotaDebito', 'Interes', 'PagoAplicado', 'NotaCredito', 'Retencion', 'Castigo', 'Anticipo'))
);
GO

CREATE INDEX [IX_Movimiento_Tercero]
    ON [cartera].[Movimiento] ([TenantId], [EmpresaId], [TipoCartera], [TerceroId], [FechaMovimiento]);
GO

CREATE TABLE [cartera].[Pago]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Pago] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TipoCartera] varchar(3) NOT NULL,
    [TerceroId] uniqueidentifier NOT NULL,
    [Numero] varchar(40) NOT NULL,
    [FechaPago] date NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [ImportePago] decimal(28, 10) NOT NULL,
    [ImporteAnticipo] decimal(28, 10) NOT NULL CONSTRAINT [DF_Pago_Anticipo] DEFAULT (0),
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Pago_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_Pago_TipoCartera] CHECK ([TipoCartera] IN ('CxC', 'CxP')),
    CONSTRAINT [CK_Pago_Importe] CHECK ([ImportePago] > 0),
    CONSTRAINT [CK_Pago_Anticipo] CHECK ([ImporteAnticipo] >= 0)
);
GO

CREATE TABLE [cartera].[AplicacionPago]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AplicacionPago] PRIMARY KEY,
    [PagoId] uniqueidentifier NOT NULL,
    [DocumentoId] uniqueidentifier NOT NULL,
    [CuotaId] uniqueidentifier NULL,
    [ImporteAplicado] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_AplicacionPago_Pago] FOREIGN KEY ([PagoId]) REFERENCES [cartera].[Pago] ([Id]),
    CONSTRAINT [FK_AplicacionPago_Documento] FOREIGN KEY ([DocumentoId]) REFERENCES [cartera].[Documento] ([Id]),
    CONSTRAINT [FK_AplicacionPago_Cuota] FOREIGN KEY ([CuotaId]) REFERENCES [cartera].[Cuota] ([Id]),
    CONSTRAINT [CK_AplicacionPago_Importe] CHECK ([ImporteAplicado] > 0)
);
GO

CREATE TABLE [cartera].[ProyeccionSaldo]
(
    [DocumentoId] uniqueidentifier NOT NULL CONSTRAINT [PK_ProyeccionSaldo] PRIMARY KEY,
    [Saldo] decimal(28, 10) NOT NULL,
    [ActualizadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProyeccionSaldo_Actualizado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_ProyeccionSaldo_Documento] FOREIGN KEY ([DocumentoId]) REFERENCES [cartera].[Documento] ([Id])
);
GO
