namespace Neros.Domain.Proyectos;

public static class MaquinaEstadosProyecto
{
    public static EstadoProyecto Activar(EstadoProyecto actual)
    {
        if (actual is not (EstadoProyecto.Planificado or EstadoProyecto.Suspendido))
            throw new InvalidOperationException("Solo un proyecto planificado o suspendido puede activarse.");
        return EstadoProyecto.Activo;
    }

    public static EstadoProyecto Suspender(EstadoProyecto actual)
    {
        if (actual != EstadoProyecto.Activo)
            throw new InvalidOperationException("Solo un proyecto activo puede suspenderse.");
        return EstadoProyecto.Suspendido;
    }

    public static EstadoProyecto Cerrar(EstadoProyecto actual)
    {
        if (actual is not (EstadoProyecto.Activo or EstadoProyecto.Suspendido))
            throw new InvalidOperationException("Solo un proyecto activo o suspendido puede cerrarse.");
        return EstadoProyecto.Cerrado;
    }

    public static EstadoProyecto Cancelar(EstadoProyecto actual)
    {
        if (actual is EstadoProyecto.Cerrado or EstadoProyecto.Cancelado)
            throw new InvalidOperationException("El proyecto ya esta cerrado o cancelado.");
        return EstadoProyecto.Cancelado;
    }

    public static void ValidarAceptaImputaciones(EstadoProyecto actual)
    {
        if (actual is not (EstadoProyecto.Activo or EstadoProyecto.Planificado))
            throw new InvalidOperationException("El proyecto no acepta imputaciones en su estado actual.");
    }
}
