/*
===============================================================================
Neros ERP
Script        : S0001__catalogo_demo_retail.sql
Modulo        : inventario
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Producto y bodega demo para RETAIL01.
Dependencias  : V0001__esquema_inventario.sql
Objetos       : inventario.Producto, Bodega, Existencia
Motivo        : Datos operativos locales.
Impacto       : Inserta catálogo mínimo.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

DECLARE @Empresa uniqueidentifier = '11111111-1111-4111-8111-111111110001';
DECLARE @Producto uniqueidentifier = '55555555-5555-4555-8555-111111110001';
DECLARE @Bodega uniqueidentifier = '66666666-6666-4666-8666-111111110001';

IF NOT EXISTS (SELECT 1 FROM [inventario].[Producto] WHERE [EmpresaId]=@Empresa AND [Codigo]='SKU-001')
    INSERT INTO [inventario].[Producto] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre])
    VALUES (@Producto,@Empresa,@Empresa,'SKU-001',N'Producto demo retail');

SELECT @Producto = [Id] FROM [inventario].[Producto] WHERE [EmpresaId]=@Empresa AND [Codigo]='SKU-001';

IF NOT EXISTS (SELECT 1 FROM [inventario].[Bodega] WHERE [EmpresaId]=@Empresa AND [Codigo]='PRIN')
    INSERT INTO [inventario].[Bodega] ([Id],[TenantId],[EmpresaId],[Codigo],[Nombre])
    VALUES (@Bodega,@Empresa,@Empresa,'PRIN',N'Bodega principal');

SELECT @Bodega = [Id] FROM [inventario].[Bodega] WHERE [EmpresaId]=@Empresa AND [Codigo]='PRIN';

IF NOT EXISTS (SELECT 1 FROM [inventario].[Existencia] WHERE [ProductoId]=@Producto AND [BodegaId]=@Bodega)
    INSERT INTO [inventario].[Existencia] ([ProductoId],[BodegaId],[Cantidad],[ValorTotal],[CostoPromedio])
    VALUES (@Producto,@Bodega,100,0,0);
GO
