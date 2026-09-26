namespace Neros.Domain.Presupuesto;

public static class MaquinaEstadosVersionPresupuesto
{
    public static EstadoVersionPresupuesto Aprobar(EstadoVersionPresupuesto actual)
    {
        if (actual != EstadoVersionPresupuesto.Borrador)
            throw new InvalidOperationException("Solo una version en borrador puede aprobarse.");
        return EstadoVersionPresupuesto.Aprobada;
    }

    public static EstadoVersionPresupuesto Cerrar(EstadoVersionPresupuesto actual)
    {
        if (actual != EstadoVersionPresupuesto.Aprobada)
            throw new InvalidOperationException("Solo una version aprobada puede cerrarse.");
        return EstadoVersionPresupuesto.Cerrada;
    }

    public static void ValidarEditable(EstadoVersionPresupuesto actual)
    {
        if (actual != EstadoVersionPresupuesto.Borrador)
            throw new InvalidOperationException("Solo una version en borrador admite cambios de lineas.");
    }
}
