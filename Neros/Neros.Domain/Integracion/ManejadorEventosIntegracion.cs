using Neros.Domain.Globalizacion;

namespace Neros.Domain.Integracion;

public static class ManejadorEventosIntegracion
{
    public static IReadOnlyList<EfectoIntegracionProcesado> Manejar(string consumidor, EventoIntegracionEntrada evento)
    {
        ArgumentNullException.ThrowIfNull(evento);
        return consumidor switch
        {
            MotorEnrutamientoIntegracion.ConsumidorInventarioReservas => ManejarReservas(evento),
            MotorEnrutamientoIntegracion.ConsumidorInventarioEntradas => ManejarEntradas(evento),
            MotorEnrutamientoIntegracion.ConsumidorContabilidad => ManejarContabilidad(evento),
            MotorEnrutamientoIntegracion.ConsumidorBusqueda => ManejarBusqueda(evento),
            _ => []
        };
    }

    private static IReadOnlyList<EfectoIntegracionProcesado> ManejarReservas(EventoIntegracionEntrada evento)
    {
        if (evento.Carga is PedidoConfirmadoIntegracion pedido)
        {
            var reserva = FlujosIntegracion.PedidoConfirmadoAReserva(pedido);
            return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.ReservaInventario,
                $"Reserva pedido {reserva.PedidoId} ({reserva.Lineas.Count} lineas)")];
        }

        if (evento.Carga is ReservaInventarioSolicitada reservaDirecta)
            return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.ReservaInventario,
                $"Reserva pedido {reservaDirecta.PedidoId} ({reservaDirecta.Lineas.Count} lineas)")];

        return [];
    }

    private static IReadOnlyList<EfectoIntegracionProcesado> ManejarEntradas(EventoIntegracionEntrada evento)
    {
        if (evento.Carga is not RecepcionCompraIntegracion recepcion)
            return [];

        var entrada = FlujosIntegracion.MapearRecepcion(recepcion);
        var movimientos = FlujosIntegracion.RecepcionAEntradasInventario(
            entrada, DateTime.UtcNow);
        return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.MovimientoInventario,
            $"{movimientos.Count} movimiento(s) recepcion {recepcion.RecepcionId}")];
    }

    private static IReadOnlyList<EfectoIntegracionProcesado> ManejarContabilidad(EventoIntegracionEntrada evento)
    {
        var politica = CatalogoIso.PoliticaPorDefecto("COP");
        if (evento.Carga is ContabilizacionPorRecepcion recepcion)
        {
            var reglas = FlujosIntegracion.RecepcionAReglasContables(recepcion, politica);
            return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.ReglasContables,
                $"{reglas.Count} lineas contables recepcion {recepcion.RecepcionId}")];
        }

        if (evento.Carga is ContabilizacionPorNomina nomina)
        {
            var reglas = FlujosIntegracion.NominaAReglasContables(nomina, politica);
            return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.ReglasContables,
                $"{reglas.Count} lineas contables nomina {nomina.LiquidacionId}")];
        }

        if (evento.Carga is SolicitudContabilizacionIntegracion solicitud)
            return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.ContabilizacionPendiente,
                $"Posting {solicitud.OrigenEvento} agregado {solicitud.AggregateId}")];

        if (evento.Carga is RecepcionCompraIntegracion recepcionCompra)
        {
            var contabilizacion = FlujosIntegracion.RecepcionAContabilizacion(recepcionCompra);
            var reglas = FlujosIntegracion.RecepcionAReglasContables(contabilizacion, politica);
            return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.ReglasContables,
                $"{reglas.Count} lineas contables recepcion {recepcionCompra.RecepcionId}")];
        }

        return [];
    }

    private static IReadOnlyList<EfectoIntegracionProcesado> ManejarBusqueda(EventoIntegracionEntrada evento)
    {
        if (!MotorEnrutamientoIntegracion.EsEventoIndexable(evento.TipoEvento))
            return [];
        return [new EfectoIntegracionProcesado(ClaseEfectoIntegracion.IndexacionBusqueda,
            $"Indexar {evento.TipoEvento} agregado {evento.AggregateId}")];
    }
}
