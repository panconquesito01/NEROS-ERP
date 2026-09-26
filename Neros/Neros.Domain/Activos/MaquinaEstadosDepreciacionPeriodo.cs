namespace Neros.Domain.Activos;

public static class MaquinaEstadosDepreciacionPeriodo
{
    public static EstadoDepreciacionPeriodo Contabilizar(EstadoDepreciacionPeriodo actual)
    {
        if (actual != EstadoDepreciacionPeriodo.Calculada)
            throw new InvalidOperationException("Solo una depreciacion calculada puede contabilizarse.");
        return EstadoDepreciacionPeriodo.Contabilizada;
    }
}
