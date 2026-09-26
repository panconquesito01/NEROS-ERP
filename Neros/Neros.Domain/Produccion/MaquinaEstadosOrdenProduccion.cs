namespace Neros.Domain.Produccion;

public static class MaquinaEstadosOrdenProduccion
{
    public static EstadoOrdenProduccion Liberar(EstadoOrdenProduccion actual)
    {
        if (actual != EstadoOrdenProduccion.Planificada)
            throw new InvalidOperationException("Solo una orden planificada puede liberarse.");
        return EstadoOrdenProduccion.Liberada;
    }

    public static EstadoOrdenProduccion Iniciar(EstadoOrdenProduccion actual)
    {
        if (actual is not (EstadoOrdenProduccion.Liberada or EstadoOrdenProduccion.EnProceso))
            throw new InvalidOperationException("La orden debe estar liberada para iniciar produccion.");
        return EstadoOrdenProduccion.EnProceso;
    }

    public static EstadoOrdenProduccion Terminar(EstadoOrdenProduccion actual)
    {
        if (actual != EstadoOrdenProduccion.EnProceso)
            throw new InvalidOperationException("Solo una orden en proceso puede terminarse.");
        return EstadoOrdenProduccion.Terminada;
    }

    public static EstadoOrdenProduccion Cerrar(EstadoOrdenProduccion actual)
    {
        if (actual != EstadoOrdenProduccion.Terminada)
            throw new InvalidOperationException("Solo una orden terminada puede cerrarse.");
        return EstadoOrdenProduccion.Cerrada;
    }

    public static void ValidarMovimiento(EstadoOrdenProduccion actual, TipoMovimientoProduccion tipo)
    {
        if (actual is EstadoOrdenProduccion.Cerrada or EstadoOrdenProduccion.Anulada or EstadoOrdenProduccion.Planificada)
            throw new InvalidOperationException("La orden no admite movimientos en su estado actual.");
        if (tipo is TipoMovimientoProduccion.Terminado or TipoMovimientoProduccion.Merma
            && actual is not (EstadoOrdenProduccion.EnProceso or EstadoOrdenProduccion.Terminada))
            throw new InvalidOperationException("Terminados y merma requieren orden en proceso o terminada.");
    }
}
