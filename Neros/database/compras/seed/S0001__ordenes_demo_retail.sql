/*
===============================================================================
Neros ERP
Script        : S0001__ordenes_demo_retail.sql
Modulo        : compras
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Orden de compra demo para RETAIL01 (recepciones).
Dependencias  : V0001__esquema_compras.sql
Objetos       : compras.OrdenCompra, OrdenCompraLinea
Motivo        : Habilitar recepciones demo.
Impacto       : Inserta orden confirmada.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : Ninguna
===============================================================================
*/

DECLARE @Empresa uniqueidentifier = '11111111-1111-4111-8111-111111110001';
DECLARE @Tercero uniqueidentifier = '00000000-0000-4000-8000-000000000001';
DECLARE @Orden uniqueidentifier = '99999999-9999-4999-8999-111111110001';
DECLARE @Linea uniqueidentifier = 'aaaaaaaa-aaaa-4aaa-8aaa-999999990001';

IF NOT EXISTS (SELECT 1 FROM [compras].[OrdenCompra] WHERE [Id]=@Orden)
BEGIN
    INSERT INTO [compras].[OrdenCompra]
        ([Id],[TenantId],[EmpresaId],[Numero],[Estado],[FechaDocumento],[MonedaCodigo],[ProveedorTerceroId],[ProveedorRazonSocial],[Subtotal],[Total],[CreadoPorUsuarioId])
    VALUES (@Orden,@Empresa,@Empresa,'OC-DEMO-001','Borrador',CAST(SYSUTCDATETIME() AS date),'COP',@Tercero,N'Proveedor demo',120000,120000,@Tercero);
    INSERT INTO [compras].[OrdenCompraLinea]
        ([Id],[OrdenCompraId],[LineaNumero],[Descripcion],[CantidadPedida],[PrecioUnitario],[Bruto],[BaseNeta])
    VALUES (@Linea,@Orden,1,N'Insumo demo',10,12000,120000,120000);
END
GO
