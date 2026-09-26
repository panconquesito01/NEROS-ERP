namespace Neros.Domain.Activos;

public static class MaquinaEstadosActivoFijo
{
    public static EstadoActivoFijo Activar(EstadoActivoFijo actual)
    {
        if (actual != EstadoActivoFijo.Borrador)
            throw new InvalidOperationException("Solo un activo en borrador puede activarse.");
        return EstadoActivoFijo.Activo;
    }

    public static EstadoActivoFijo DarDeBaja(EstadoActivoFijo actual)
    {
        if (actual != EstadoActivoFijo.Activo)
            throw new InvalidOperationException("Solo un activo activo puede darse de baja.");
        return EstadoActivoFijo.DadoDeBaja;
    }

    public static void ValidarOperacionDepreciacion(EstadoActivoFijo actual)
    {
        if (actual != EstadoActivoFijo.Activo)
            throw new InvalidOperationException("Solo un activo activo admite depreciacion.");
    }
}
