using Neros.Domain.Contabilidad;
using Neros.Domain.Globalizacion;
using Neros.Domain.Inventario;

namespace Neros.Domain.Integracion;

/// <summary>Flujos pedido→reserva, recepcion→existencias, evento→contabilizacion (fase 13).</summary>
public static class FlujosIntegracion
{
    public static ReservaInventarioSolicitada PedidoConfirmadoAReserva(PedidoConfirmadoIntegracion pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        if (pedido.Lineas.Count == 0) throw new ArgumentException("Pedido sin lineas.", nameof(pedido));
        foreach (var linea in pedido.Lineas)
        {
            if (linea.Cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(pedido));
            if (linea.ProductoReferenciaId == Guid.Empty || linea.BodegaId == Guid.Empty)
                throw new ArgumentException("Producto y bodega requeridos para reservar.");
        }
        return new ReservaInventarioSolicitada(pedido.PedidoId, pedido.TenantId, pedido.EmpresaId, pedido.Lineas);
    }

    public static IReadOnlyList<MovimientoInventarioPendiente> RecepcionAEntradasInventario(
        EntradaInventarioPorRecepcion entrada, DateTime fechaNegocioUtc)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        if (entrada.Lineas.Count == 0) throw new ArgumentException("Recepcion sin lineas.", nameof(entrada));
        var movimientos = new List<MovimientoInventarioPendiente>();
        foreach (var linea in entrada.Lineas)
        {
            if (linea.CantidadRecibida <= 0 || linea.CostoUnitario < 0)
                throw new ArgumentOutOfRangeException(nameof(entrada));
            movimientos.Add(new MovimientoInventarioPendiente(
                Guid.NewGuid(),
                DateOnly.FromDateTime(fechaNegocioUtc),
                movimientos.Count + 1,
                TipoMovimientoInventario.Entrada,
                EsEntrada: true,
                linea.CantidadRecibida,
                linea.CostoUnitario));
        }
        return movimientos;
    }

    public static IReadOnlyList<MotorContabilizacion.LineaRegla> RecepcionAReglasContables(
        ContabilizacionPorRecepcion evento, PoliticaRedondeo politica)
        => MotorContabilizacion.ReglasCompraRecibida(
            new MotorContabilizacion.EventoCompraRecibida(evento.TotalCompra, evento.BaseGravable, evento.IvaDescontable),
            politica);

    public static EntradaInventarioPorRecepcion MapearRecepcion(RecepcionCompraIntegracion recepcion)
    {
        ArgumentNullException.ThrowIfNull(recepcion);
        return new EntradaInventarioPorRecepcion(recepcion.RecepcionId, recepcion.OrdenCompraId, recepcion.Lineas);
    }

    public static ContabilizacionPorRecepcion RecepcionAContabilizacion(RecepcionCompraIntegracion recepcion)
    {
        ArgumentNullException.ThrowIfNull(recepcion);
        if (recepcion.Lineas.Count == 0) throw new ArgumentException("Recepcion sin lineas.", nameof(recepcion));
        decimal baseGravable = 0;
        foreach (var linea in recepcion.Lineas)
            baseGravable += linea.CantidadRecibida * linea.CostoUnitario;
        var iva = Math.Round(baseGravable * 0.19m, 2, MidpointRounding.AwayFromZero);
        return new ContabilizacionPorRecepcion(
            recepcion.OrdenCompraId, recepcion.RecepcionId, baseGravable + iva, baseGravable, iva);
    }

    public static IReadOnlyList<MotorContabilizacion.LineaRegla> NominaAReglasContables(
        ContabilizacionPorNomina evento, PoliticaRedondeo politica)
    {
        ArgumentNullException.ThrowIfNull(evento);
        if (evento.NetoPagar < 0) throw new ArgumentOutOfRangeException(nameof(evento));
        return MotorContabilizacion.ReglasNominaLiquidada(
            new MotorContabilizacion.EventoNominaLiquidada(
                evento.TotalDevengos, evento.TotalDeducciones, evento.NetoPagar),
            politica);
    }
}
