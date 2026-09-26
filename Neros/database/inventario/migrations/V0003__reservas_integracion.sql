/*
===============================================================================
Neros ERP
Script        : V0003__reservas_integracion.sql
Modulo        : inventario
Fecha         : 2026-09-26
Autor         : Equipo Neros
Descripcion   : Reservas por pedido e idempotencia de eventos de integracion (D-03).
Dependencias  : V0002__integracion_outbox_inbox.sql
Objetos       : inventario.ReservaInventario
Motivo        : Persistir efecto del consumidor inventario.reservas.
Impacto       : Tabla nueva.
Destructivo   : NO
Transaccional : SI
Riesgo        : BAJO
Rollback      : Roll forward.
Ticket/ADR    : ADR-0004
Validacion    : validation/003_reservas_integracion.sql
===============================================================================
*/

CREATE TABLE [inventario].[ReservaInventario]
(
    [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_ReservaInventario] PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [PedidoId] uniqueidentifier NOT NULL,
    [ProductoId] uniqueidentifier NOT NULL,
    [BodegaId] uniqueidentifier NOT NULL,
    [Cantidad] decimal(28, 10) NOT NULL,
    [EventoIntegracionId] uniqueidentifier NOT NULL,
    [CreadoEnUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Reserva_Creado] DEFAULT (sysutcdatetime()),
    CONSTRAINT [FK_Reserva_Producto] FOREIGN KEY ([ProductoId]) REFERENCES [inventario].[Producto] ([Id]),
    CONSTRAINT [FK_Reserva_Bodega] FOREIGN KEY ([BodegaId]) REFERENCES [inventario].[Bodega] ([Id]),
    CONSTRAINT [CK_Reserva_Cantidad] CHECK ([Cantidad] > 0)
);
GO

CREATE UNIQUE INDEX [UX_Reserva_Pedido_Linea]
    ON [inventario].[ReservaInventario] ([PedidoId], [ProductoId], [BodegaId]);
GO

CREATE UNIQUE INDEX [UX_Reserva_EventoIntegracion]
    ON [inventario].[ReservaInventario] ([EventoIntegracionId]);
GO
