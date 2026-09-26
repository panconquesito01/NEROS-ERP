/*
===============================================================================
Neros ERP
Script        : V0001__esquema_nomina_base.sql
Modulo        : nomina
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Empleados, contratos, conceptos, periodos y liquidacion con snapshot (plan fase 18, §53).
Dependencias  : Ninguna
Objetos       : nomina.Empleado, Contrato, ConceptoNomina, PeriodoNomina, LiquidacionNomina, LiquidacionSnapshot, LiquidacionLinea
Motivo        : Base de nomina segregada de Terceros.
Impacto       : Esquema y tablas nuevas.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/001_esquema_nomina.sql
===============================================================================
*/

CREATE SCHEMA [nomina];
GO

CREATE TABLE [nomina].[Empleado]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Empleado] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [TerceroId] uniqueidentifier NOT NULL,
    [CodigoEmpleado] varchar(30) NOT NULL,
    [FechaIngreso] date NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Empleado_Estado] DEFAULT ('Activo'),
    [CuentaNominaBanco] nvarchar(80) NULL,
    [CuentaNominaTipo] varchar(20) NULL,
    [CuentaNominaNumero] varchar(40) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Empleado_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [CK_Empleado_Estado] CHECK ([Estado] IN ('Activo', 'Retirado'))
);
GO

CREATE UNIQUE INDEX [UX_Empleado_Empresa_Tercero]
    ON [nomina].[Empleado] ([TenantId], [EmpresaId], [TerceroId]);
GO

CREATE UNIQUE INDEX [UX_Empleado_Empresa_Codigo]
    ON [nomina].[Empleado] ([TenantId], [EmpresaId], [CodigoEmpleado]);
GO

CREATE TABLE [nomina].[Contrato]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Contrato] PRIMARY KEY,
    [EmpleadoId] uniqueidentifier NOT NULL,
    [FechaInicio] date NOT NULL,
    [FechaFin] date NULL,
    [TipoContrato] varchar(30) NOT NULL,
    [SalarioBase] decimal(28, 10) NOT NULL,
    [HorasSemanales] decimal(8, 2) NOT NULL CONSTRAINT [DF_Contrato_HorasSem] DEFAULT (48),
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Contrato_Estado] DEFAULT ('Activo'),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Contrato_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Contrato_Empleado] FOREIGN KEY ([EmpleadoId]) REFERENCES [nomina].[Empleado] ([Id]),
    CONSTRAINT [CK_Contrato_Salario] CHECK ([SalarioBase] >= 0),
    CONSTRAINT [CK_Contrato_Horas] CHECK ([HorasSemanales] > 0),
    CONSTRAINT [CK_Contrato_Vigencia] CHECK ([FechaFin] IS NULL OR [FechaFin] >= [FechaInicio]),
    CONSTRAINT [CK_Contrato_Estado] CHECK ([Estado] IN ('Activo', 'Finalizado'))
);
GO

CREATE INDEX [IX_Contrato_Empleado_Estado]
    ON [nomina].[Contrato] ([EmpleadoId], [Estado], [FechaInicio] DESC);
GO

CREATE TABLE [nomina].[ConceptoNomina]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ConceptoNomina] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Codigo] varchar(30) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [Naturaleza] varchar(15) NOT NULL,
    [TipoFormula] varchar(30) NOT NULL,
    [ValorFijo] decimal(28, 10) NULL,
    [PorcentajeSalario] decimal(28, 10) NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_Concepto_Activo] DEFAULT (1),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Concepto_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_Concepto_Naturaleza] CHECK ([Naturaleza] IN ('Devengo', 'Deduccion')),
    CONSTRAINT [CK_Concepto_TipoFormula] CHECK ([TipoFormula] IN ('Fijo', 'PorcentajeSalario', 'Horas', 'Manual'))
);
GO

CREATE UNIQUE INDEX [UX_Concepto_Empresa_Codigo]
    ON [nomina].[ConceptoNomina] ([TenantId], [EmpresaId], [Codigo]);
GO

CREATE TABLE [nomina].[PeriodoNomina]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PeriodoNomina] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [Anio] int NOT NULL,
    [Mes] int NOT NULL,
    [FechaInicio] date NOT NULL,
    [FechaFin] date NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Periodo_Estado] DEFAULT ('Abierto'),
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Periodo_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [CK_Periodo_Mes] CHECK ([Mes] BETWEEN 1 AND 12),
    CONSTRAINT [CK_Periodo_Vigencia] CHECK ([FechaFin] >= [FechaInicio]),
    CONSTRAINT [CK_Periodo_Estado] CHECK ([Estado] IN ('Abierto', 'Cerrado'))
);
GO

CREATE UNIQUE INDEX [UX_Periodo_Empresa_AnioMes]
    ON [nomina].[PeriodoNomina] ([TenantId], [EmpresaId], [Anio], [Mes]);
GO

CREATE TABLE [nomina].[LiquidacionNomina]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_LiquidacionNomina] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [PeriodoNominaId] uniqueidentifier NOT NULL,
    [EmpleadoId] uniqueidentifier NOT NULL,
    [ContratoId] uniqueidentifier NOT NULL,
    [Estado] varchar(20) NOT NULL CONSTRAINT [DF_Liquidacion_Estado] DEFAULT ('Borrador'),
    [DiasTrabajados] decimal(8, 2) NOT NULL CONSTRAINT [DF_Liquidacion_Dias] DEFAULT (0),
    [HorasTrabajadas] decimal(8, 2) NOT NULL CONSTRAINT [DF_Liquidacion_Horas] DEFAULT (0),
    [TotalDevengos] decimal(28, 10) NOT NULL CONSTRAINT [DF_Liquidacion_Devengos] DEFAULT (0),
    [TotalDeducciones] decimal(28, 10) NOT NULL CONSTRAINT [DF_Liquidacion_Deducciones] DEFAULT (0),
    [NetoPagar] decimal(28, 10) NOT NULL CONSTRAINT [DF_Liquidacion_Neto] DEFAULT (0),
    [CalculadaEnUtc] datetime2(3) NULL,
    [ContabilizadaEnUtc] datetime2(3) NULL,
    [AnuladaEnUtc] datetime2(3) NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Liquidacion_Creado] DEFAULT (sysutcdatetime()),
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [FK_Liquidacion_Periodo] FOREIGN KEY ([PeriodoNominaId]) REFERENCES [nomina].[PeriodoNomina] ([Id]),
    CONSTRAINT [FK_Liquidacion_Empleado] FOREIGN KEY ([EmpleadoId]) REFERENCES [nomina].[Empleado] ([Id]),
    CONSTRAINT [FK_Liquidacion_Contrato] FOREIGN KEY ([ContratoId]) REFERENCES [nomina].[Contrato] ([Id]),
    CONSTRAINT [CK_Liquidacion_Estado] CHECK ([Estado] IN ('Borrador', 'Calculada', 'Contabilizada', 'Anulada')),
    CONSTRAINT [CK_Liquidacion_Dias] CHECK ([DiasTrabajados] >= 0),
    CONSTRAINT [CK_Liquidacion_Horas] CHECK ([HorasTrabajadas] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_Liquidacion_Periodo_Empleado]
    ON [nomina].[LiquidacionNomina] ([PeriodoNominaId], [EmpleadoId]);
GO

CREATE TABLE [nomina].[LiquidacionSnapshot]
(
    [LiquidacionNominaId] uniqueidentifier NOT NULL CONSTRAINT [PK_LiquidacionSnapshot] PRIMARY KEY,
    [ContratoId] uniqueidentifier NOT NULL,
    [TipoContrato] varchar(30) NOT NULL,
    [SalarioBase] decimal(28, 10) NOT NULL,
    [FechaInicioContrato] date NOT NULL,
    [FechaFinContrato] date NULL,
    [DiasTrabajados] decimal(8, 2) NOT NULL,
    [HorasTrabajadas] decimal(8, 2) NOT NULL,
    [VersionReglasNomina] nvarchar(40) NOT NULL,
    [PaqueteLegalCodigo] varchar(30) NULL,
    [AplicaReglasLegalesColombia] bit NOT NULL CONSTRAINT [DF_LiqSnap_Legal] DEFAULT (0),
    [VersionSoftware] nvarchar(40) NOT NULL,
    CONSTRAINT [FK_LiquidacionSnapshot_Liquidacion] FOREIGN KEY ([LiquidacionNominaId]) REFERENCES [nomina].[LiquidacionNomina] ([Id])
);
GO

CREATE TABLE [nomina].[LiquidacionLinea]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_LiquidacionLinea] PRIMARY KEY,
    [LiquidacionNominaId] uniqueidentifier NOT NULL,
    [LineaNumero] int NOT NULL,
    [ConceptoNominaId] uniqueidentifier NULL,
    [ConceptoCodigo] varchar(30) NOT NULL,
    [ConceptoNombre] nvarchar(120) NOT NULL,
    [Naturaleza] varchar(15) NOT NULL,
    [TipoFormula] varchar(30) NOT NULL,
    [BaseCalculo] decimal(28, 10) NOT NULL CONSTRAINT [DF_LiqLinea_Base] DEFAULT (0),
    [Tarifa] decimal(28, 10) NOT NULL CONSTRAINT [DF_LiqLinea_Tarifa] DEFAULT (0),
    [Importe] decimal(28, 10) NOT NULL,
    CONSTRAINT [FK_LiquidacionLinea_Liquidacion] FOREIGN KEY ([LiquidacionNominaId]) REFERENCES [nomina].[LiquidacionNomina] ([Id]),
    CONSTRAINT [CK_LiquidacionLinea_Naturaleza] CHECK ([Naturaleza] IN ('Devengo', 'Deduccion')),
    CONSTRAINT [CK_LiquidacionLinea_Importe] CHECK ([Importe] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_LiquidacionLinea_Numero]
    ON [nomina].[LiquidacionLinea] ([LiquidacionNominaId], [LineaNumero]);
GO
