/*
===============================================================================
Neros ERP
Script        : S0001__documentos_demo_retail.sql
Modulo        : ventas
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Cotización y pedido demo confirmados para RETAIL01.
Dependencias  : V0001__esquema_ventas.sql
Objetos       : ventas.Cotizacion, CotizacionLinea, Pedido, PedidoLinea
Motivo        : Listados con datos en desarrollo.
Impacto       : Inserta documentos de ejemplo.
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
DECLARE @Cot uniqueidentifier = '77777777-7777-4777-8777-111111110001';
DECLARE @Ped uniqueidentifier = '88888888-8888-4888-8888-111111110001';

UPDATE [ventas].[Cotizacion] SET [Estado]='Borrador' WHERE [Id]=@Cot AND [Estado]='Confirmado';
UPDATE [ventas].[Pedido] SET [Estado]='Borrador' WHERE [Id]=@Ped AND [Estado]='Confirmado';

IF NOT EXISTS (SELECT 1 FROM [ventas].[Cotizacion] WHERE [Id]=@Cot)
BEGIN
    INSERT INTO [ventas].[Cotizacion]
        ([Id],[TenantId],[EmpresaId],[Numero],[Estado],[FechaDocumento],[MonedaCodigo],[ClienteTerceroId],[ClienteRazonSocial],[Subtotal],[Total])
    VALUES (@Cot,@Empresa,@Empresa,'COT-DEMO-001','Borrador',CAST(SYSUTCDATETIME() AS date),'COP',@Tercero,N'Cliente demo retail',250000,250000);
    INSERT INTO [ventas].[CotizacionLinea]
        ([Id],[CotizacionId],[LineaNumero],[Descripcion],[Cantidad],[PrecioUnitario],[Bruto],[BaseNeta])
    VALUES (NEWID(),@Cot,1,N'Servicio demostración',1,250000,250000,250000);
END

IF NOT EXISTS (SELECT 1 FROM [ventas].[Pedido] WHERE [Id]=@Ped)
BEGIN
    INSERT INTO [ventas].[Pedido]
        ([Id],[TenantId],[EmpresaId],[Numero],[Estado],[FechaDocumento],[MonedaCodigo],[ClienteTerceroId],[ClienteRazonSocial],[Subtotal],[Total])
    VALUES (@Ped,@Empresa,@Empresa,'PED-DEMO-001','Borrador',CAST(SYSUTCDATETIME() AS date),'COP',@Tercero,N'Cliente demo retail',180000,180000);
    INSERT INTO [ventas].[PedidoLinea]
        ([Id],[PedidoId],[LineaNumero],[Descripcion],[Cantidad],[PrecioUnitario],[Bruto],[BaseNeta])
    VALUES (NEWID(),@Ped,1,N'Pedido demostración',2,90000,180000,180000);
END
GO
