namespace Neros.Domain.Integracion;

public sealed record LineaPedidoParaReserva(Guid ProductoReferenciaId, Guid BodegaId, decimal Cantidad);

public sealed record PedidoConfirmadoIntegracion(
    Guid PedidoId,
    Guid TenantId,
    Guid EmpresaId,
    long VersionAgregado,
    IReadOnlyList<LineaPedidoParaReserva> Lineas);

public sealed record ReservaInventarioSolicitada(
    Guid PedidoId,
    Guid TenantId,
    Guid EmpresaId,
    IReadOnlyList<LineaPedidoParaReserva> Lineas);

public sealed record LineaRecepcionParaInventario(
    Guid ProductoReferenciaId,
    Guid BodegaId,
    decimal CantidadRecibida,
    decimal CostoUnitario);

public sealed record RecepcionCompraIntegracion(
    Guid RecepcionId,
    Guid OrdenCompraId,
    Guid TenantId,
    Guid EmpresaId,
    long VersionAgregado,
    IReadOnlyList<LineaRecepcionParaInventario> Lineas);

public sealed record EntradaInventarioPorRecepcion(
    Guid RecepcionId,
    Guid OrdenCompraId,
    IReadOnlyList<LineaRecepcionParaInventario> Lineas);

public sealed record ContabilizacionPorRecepcion(
    Guid OrdenCompraId,
    Guid RecepcionId,
    decimal TotalCompra,
    decimal BaseGravable,
    decimal IvaDescontable);

public sealed record ContabilizacionPorNomina(
    Guid LiquidacionId,
    Guid PeriodoId,
    decimal TotalDevengos,
    decimal TotalDeducciones,
    decimal NetoPagar);

public sealed record SolicitudContabilizacionIntegracion(
    string OrigenEvento,
    string AggregateId,
    long VersionAgregado);
