/*
===============================================================================
Neros ERP
Script        : V0001__esquema_tesoreria.sql
Modulo        : tesoreria
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Cuentas de tesoreria, movimientos, extracto y conciliacion (plan fase 15, §44).
Dependencias  : Ninguna
Objetos       : tesoreria.Cuenta, Movimiento, Transferencia, ExtractoBancario, LineaExtracto, Conciliacion, EnlaceConciliacion
Motivo        : Base del modulo Tesoreria.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_tesoreria.sql
===============================================================================
*/

CREATE SCHEMA [tesoreria];
GO

CREATE TABLE [tesoreria].[Cuenta]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Cuenta] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Tipo] varchar(10) NOT NULL,
    [Codigo] varchar(20) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [MonedaCodigo] char(3) NOT NULL,
    [SaldoInicial] decimal(28, 10) NOT NULL CONSTRAINT [DF_Cuenta_SaldoInicial] DEFAULT (0),
    [Activa] bit NOT NULL CONSTRAINT [DF_Cuenta_Activa] DEFAULT (1),
    CONSTRAINT [CK_Cuenta_Tipo] CHECK ([Tipo] IN ('Caja', 'Banco'))
);
GO

CREATE UNIQUE INDEX [UX_Cuenta_Empresa_Codigo]
    ON [tesoreria].[Cuenta] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [tesoreria].[Movimiento]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Movimiento] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [CuentaId] uniqueidentifier NOT NULL,
    [Tipo] varchar(20) NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [FechaMovimiento] date NOT NULL,
    [Referencia] nvarchar(120) NULL,
    [CarteraPagoId] uniqueidentifier NULL,
    [RegistradoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Movimiento_Registrado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_Movimiento_Cuenta] FOREIGN KEY ([CuentaId]) REFERENCES [tesoreria].[Cuenta] ([Id]),
    CONSTRAINT [CK_Movimiento_Tipo] CHECK ([Tipo] IN ('Ingreso', 'Egreso', 'TransferenciaEntrada', 'TransferenciaSalida')),
    CONSTRAINT [CK_Movimiento_Importe] CHECK ([Importe] > 0)
);
GO

CREATE INDEX [IX_Movimiento_Cuenta_Fecha]
    ON [tesoreria].[Movimiento] ([CuentaId], [FechaMovimiento]);
GO

CREATE TABLE [tesoreria].[Transferencia]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Transferencia] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [CuentaOrigenId] uniqueidentifier NOT NULL,
    [CuentaDestinoId] uniqueidentifier NOT NULL,
    [MovimientoSalidaId] uniqueidentifier NOT NULL,
    [MovimientoEntradaId] uniqueidentifier NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [FechaTransferencia] date NOT NULL,
    CONSTRAINT [FK_Transferencia_Origen] FOREIGN KEY ([CuentaOrigenId]) REFERENCES [tesoreria].[Cuenta] ([Id]),
    CONSTRAINT [FK_Transferencia_Destino] FOREIGN KEY ([CuentaDestinoId]) REFERENCES [tesoreria].[Cuenta] ([Id]),
    CONSTRAINT [FK_Transferencia_Salida] FOREIGN KEY ([MovimientoSalidaId]) REFERENCES [tesoreria].[Movimiento] ([Id]),
    CONSTRAINT [FK_Transferencia_Entrada] FOREIGN KEY ([MovimientoEntradaId]) REFERENCES [tesoreria].[Movimiento] ([Id]),
    CONSTRAINT [CK_Transferencia_Importe] CHECK ([Importe] > 0),
    CONSTRAINT [CK_Transferencia_CuentasDistintas] CHECK ([CuentaOrigenId] <> [CuentaDestinoId])
);
GO

CREATE TABLE [tesoreria].[ExtractoBancario]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ExtractoBancario] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [CuentaId] uniqueidentifier NOT NULL,
    [FechaDesde] date NOT NULL,
    [FechaHasta] date NOT NULL,
    [SaldoInicialExtracto] decimal(28, 10) NOT NULL,
    [SaldoFinalExtracto] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_Extracto_Cuenta] FOREIGN KEY ([CuentaId]) REFERENCES [tesoreria].[Cuenta] ([Id]),
    CONSTRAINT [CK_Extracto_Rango] CHECK ([FechaHasta] >= [FechaDesde])
);
GO

CREATE TABLE [tesoreria].[LineaExtracto]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_LineaExtracto] PRIMARY KEY,
    [ExtractoId] uniqueidentifier NOT NULL,
    [Fecha] date NOT NULL,
    [Descripcion] nvarchar(240) NOT NULL,
    [Importe] decimal(28, 10) NOT NULL,
    [EsCredito] bit NOT NULL,
    CONSTRAINT [FK_LineaExtracto_Extracto] FOREIGN KEY ([ExtractoId]) REFERENCES [tesoreria].[ExtractoBancario] ([Id])
);
GO

CREATE TABLE [tesoreria].[Conciliacion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Conciliacion] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [CuentaId] uniqueidentifier NOT NULL,
    [ExtractoId] uniqueidentifier NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Conciliacion_Estado] DEFAULT ('Borrador'),
    [SaldoLibroInicial] decimal(28, 10) NOT NULL,
    [SaldoLibroFinal] decimal(28, 10) NOT NULL,
    [SaldoExtractoFinal] decimal(28, 10) NOT NULL,
    [Diferencia] decimal(28, 10) NOT NULL,
    [CerradaEnUtc] datetime2(3) NULL,
    CONSTRAINT [FK_Conciliacion_Cuenta] FOREIGN KEY ([CuentaId]) REFERENCES [tesoreria].[Cuenta] ([Id]),
    CONSTRAINT [FK_Conciliacion_Extracto] FOREIGN KEY ([ExtractoId]) REFERENCES [tesoreria].[ExtractoBancario] ([Id]),
    CONSTRAINT [CK_Conciliacion_Estado] CHECK ([Estado] IN ('Borrador', 'Cerrada'))
);
GO

CREATE TABLE [tesoreria].[EnlaceConciliacion]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_EnlaceConciliacion] PRIMARY KEY,
    [ConciliacionId] uniqueidentifier NOT NULL,
    [MovimientoId] uniqueidentifier NULL,
    [LineaExtractoId] uniqueidentifier NULL,
    [Importe] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_Enlace_Conciliacion] FOREIGN KEY ([ConciliacionId]) REFERENCES [tesoreria].[Conciliacion] ([Id]),
    CONSTRAINT [FK_Enlace_Movimiento] FOREIGN KEY ([MovimientoId]) REFERENCES [tesoreria].[Movimiento] ([Id]),
    CONSTRAINT [FK_Enlace_Linea] FOREIGN KEY ([LineaExtractoId]) REFERENCES [tesoreria].[LineaExtracto] ([Id]),
    CONSTRAINT [CK_Enlace_AlMenosUno] CHECK ([MovimientoId] IS NOT NULL OR [LineaExtractoId] IS NOT NULL),
    CONSTRAINT [CK_Enlace_Importe] CHECK ([Importe] > 0)
);
GO
