/*
===============================================================================
Neros ERP
Script        : S0001__catalogo_contable_demo.sql
Modulo        : contabilidad
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Ejercicio, periodo, tipo de comprobante y PUC mínimo para empresas demo.
Dependencias  : V0001__esquema_contabilidad.sql
Objetos       : contabilidad.Ejercicio, Periodo, TipoComprobante, CuentaContable
Motivo        : Habilitar comprobantes borrador en desarrollo local.
Impacto       : Inserta catálogo contable base por empresa demo.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

;WITH [Demo] AS (
    SELECT v.[EmpresaId], v.[EjercicioId], v.[PeriodoId], v.[TipoId] FROM (VALUES
        ('11111111-1111-4111-8111-111111110001', 'aaaaaaaa-aaaa-4aaa-8aaa-111111110001', 'bbbbbbbb-bbbb-4bbb-8bbb-111111110001', 'cccccccc-cccc-4ccc-8ccc-111111110001'),
        ('11111111-1111-4111-8111-111111110002', 'aaaaaaaa-aaaa-4aaa-8aaa-111111110002', 'bbbbbbbb-bbbb-4bbb-8bbb-111111110002', 'cccccccc-cccc-4ccc-8ccc-111111110002'),
        ('11111111-1111-4111-8111-111111110003', 'aaaaaaaa-aaaa-4aaa-8aaa-111111110003', 'bbbbbbbb-bbbb-4bbb-8bbb-111111110003', 'cccccccc-cccc-4ccc-8ccc-111111110003')
    ) AS v([EmpresaId],[EjercicioId],[PeriodoId],[TipoId])
)
INSERT INTO [contabilidad].[Ejercicio] ([Id],[TenantId],[EmpresaId],[Anio])
SELECT d.[EjercicioId], d.[EmpresaId], d.[EmpresaId], 2026 FROM [Demo] d
WHERE NOT EXISTS (SELECT 1 FROM [contabilidad].[Ejercicio] e WHERE e.[EmpresaId]=d.[EmpresaId] AND e.[Anio]=2026);
GO

;WITH [Demo] AS (
    SELECT v.[EmpresaId], v.[EjercicioId], v.[PeriodoId] FROM (VALUES
        ('11111111-1111-4111-8111-111111110001', 'aaaaaaaa-aaaa-4aaa-8aaa-111111110001', 'bbbbbbbb-bbbb-4bbb-8bbb-111111110001'),
        ('11111111-1111-4111-8111-111111110002', 'aaaaaaaa-aaaa-4aaa-8aaa-111111110002', 'bbbbbbbb-bbbb-4bbb-8bbb-111111110002'),
        ('11111111-1111-4111-8111-111111110003', 'aaaaaaaa-aaaa-4aaa-8aaa-111111110003', 'bbbbbbbb-bbbb-4bbb-8bbb-111111110003')
    ) AS v([EmpresaId],[EjercicioId],[PeriodoId])
)
INSERT INTO [contabilidad].[Periodo] ([Id],[EjercicioId],[Numero],[FechaInicio],[FechaFin],[Estado])
SELECT d.[PeriodoId], e.[Id], 1, '2026-01-01', '2026-12-31', 'Abierto'
FROM [Demo] d
INNER JOIN [contabilidad].[Ejercicio] e ON e.[EmpresaId]=d.[EmpresaId] AND e.[Anio]=2026
WHERE NOT EXISTS (SELECT 1 FROM [contabilidad].[Periodo] p WHERE p.[EjercicioId]=e.[Id] AND p.[Numero]=1);
GO

;WITH [Demo] AS (
    SELECT v.[EmpresaId], v.[TipoId] FROM (VALUES
        ('11111111-1111-4111-8111-111111110001', 'cccccccc-cccc-4ccc-8ccc-111111110001'),
        ('11111111-1111-4111-8111-111111110002', 'cccccccc-cccc-4ccc-8ccc-111111110002'),
        ('11111111-1111-4111-8111-111111110003', 'cccccccc-cccc-4ccc-8ccc-111111110003')
    ) AS v([EmpresaId],[TipoId])
)
INSERT INTO [contabilidad].[TipoComprobante] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre])
SELECT d.[TipoId], d.[EmpresaId], d.[EmpresaId], 'CG', N'Comprobante general' FROM [Demo] d
WHERE NOT EXISTS (SELECT 1 FROM [contabilidad].[TipoComprobante] t WHERE t.[EmpresaId]=d.[EmpresaId] AND t.[Codigo]='CG');
GO

;WITH [Demo] AS (
    SELECT [EmpresaId] FROM (VALUES
        ('11111111-1111-4111-8111-111111110001'),
        ('11111111-1111-4111-8111-111111110002'),
        ('11111111-1111-4111-8111-111111110003')
    ) AS v([EmpresaId])
)
INSERT INTO [contabilidad].[CuentaContable]
    ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre],[Naturaleza],[Tipo],[Nivel],[AdmiteMovimiento])
SELECT NEWID(), d.[EmpresaId], d.[EmpresaId], '1105', N'Caja', 'D', 'Activo', 4, 1
FROM [Demo] d
WHERE NOT EXISTS (SELECT 1 FROM [contabilidad].[CuentaContable] c WHERE c.[EmpresaId]=d.[EmpresaId] AND c.[Codigo]='1105');
GO
