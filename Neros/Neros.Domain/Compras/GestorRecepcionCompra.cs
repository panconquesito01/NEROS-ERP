namespace Neros.Domain.Compras;

/// <summary>Recepciones parciales y totales sobre orden aprobada (plan §47).</summary>
public static class GestorRecepcionCompra
{
    public static ResultadoRecepcionCompra Registrar(
        EstadoOrdenCompra estadoOrden,
        IReadOnlyList<LineaOrdenParaRecepcion> lineasOrden,
        IReadOnlyList<LineaRecepcionEntrada> lineasRecepcion)
    {
        MaquinaEstadosOrdenCompra.ValidarRecepcion(estadoOrden);
        if (lineasOrden.Count == 0) throw new ArgumentException("La orden requiere lineas.", nameof(lineasOrden));
        if (lineasRecepcion.Count == 0) throw new ArgumentException("Se requiere al menos una linea recibida.", nameof(lineasRecepcion));

        var porLinea = lineasOrden.ToDictionary(l => l.LineaId);
        var actualizadas = new List<LineaOrdenRecepcionActualizada>(lineasOrden.Count);
        foreach (var entrada in lineasOrden)
        {
            actualizadas.Add(new LineaOrdenRecepcionActualizada(
                entrada.LineaId,
                entrada.CantidadPedida,
                entrada.CantidadRecibidaAcumulada,
                entrada.CantidadPedida - entrada.CantidadRecibidaAcumulada));
        }

        foreach (var recepcion in lineasRecepcion)
        {
            if (recepcion.CantidadRecibida <= 0)
                throw new ArgumentOutOfRangeException(nameof(lineasRecepcion), "Cantidad recibida debe ser positiva.");
            if (!porLinea.TryGetValue(recepcion.OrdenCompraLineaId, out var linea))
                throw new InvalidOperationException($"Linea {recepcion.OrdenCompraLineaId} no pertenece a la orden.");

            var pendiente = linea.CantidadPedida - linea.CantidadRecibidaAcumulada;
            if (recepcion.CantidadRecibida > pendiente)
                throw new InvalidOperationException($"La cantidad recibida excede la pendiente en la linea {recepcion.OrdenCompraLineaId}.");

            var nuevaAcumulada = linea.CantidadRecibidaAcumulada + recepcion.CantidadRecibida;
            porLinea[recepcion.OrdenCompraLineaId] = linea with { CantidadRecibidaAcumulada = nuevaAcumulada };
        }

        var lineasFinales = lineasOrden
            .Select(l =>
            {
                var actual = porLinea[l.LineaId];
                return new LineaOrdenRecepcionActualizada(
                    l.LineaId,
                    l.CantidadPedida,
                    actual.CantidadRecibidaAcumulada,
                    l.CantidadPedida - actual.CantidadRecibidaAcumulada);
            })
            .ToList();

        var completa = lineasFinales.All(l => l.CantidadPendiente == 0);
        return new ResultadoRecepcionCompra(MaquinaEstadosOrdenCompra.TrasRecepcion(completa), lineasFinales);
    }
}
