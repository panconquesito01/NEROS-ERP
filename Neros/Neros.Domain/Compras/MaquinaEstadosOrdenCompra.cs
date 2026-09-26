namespace Neros.Domain.Compras;

public static class MaquinaEstadosOrdenCompra
{
    public static EstadoOrdenCompra Aprobar(EstadoOrdenCompra actual)
    {
        if (actual != EstadoOrdenCompra.Borrador)
            throw new InvalidOperationException("Solo una orden en borrador puede aprobarse.");
        return EstadoOrdenCompra.Aprobada;
    }

    public static void ValidarEditable(EstadoOrdenCompra actual)
    {
        if (actual != EstadoOrdenCompra.Borrador)
            throw new InvalidOperationException("Solo una orden en borrador admite cambios.");
    }

    public static void ValidarRecepcion(EstadoOrdenCompra actual)
    {
        if (actual is not (EstadoOrdenCompra.Aprobada or EstadoOrdenCompra.RecibidaParcial))
            throw new InvalidOperationException("La orden debe estar aprobada o parcialmente recibida para registrar recepciones.");
    }

    public static EstadoOrdenCompra TrasRecepcion(bool todasLasLineasCompletas)
        => todasLasLineasCompletas ? EstadoOrdenCompra.RecibidaTotal : EstadoOrdenCompra.RecibidaParcial;
}
