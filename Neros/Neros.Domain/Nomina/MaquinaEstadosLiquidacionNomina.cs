namespace Neros.Domain.Nomina;

public static class MaquinaEstadosLiquidacionNomina
{
    public static EstadoLiquidacionNomina MarcarCalculada(EstadoLiquidacionNomina actual)
    {
        if (actual != EstadoLiquidacionNomina.Borrador)
            throw new InvalidOperationException("Solo una liquidacion en borrador puede calcularse.");
        return EstadoLiquidacionNomina.Calculada;
    }

    public static EstadoLiquidacionNomina Contabilizar(EstadoLiquidacionNomina actual)
    {
        if (actual != EstadoLiquidacionNomina.Calculada)
            throw new InvalidOperationException("Solo una liquidacion calculada puede contabilizarse.");
        return EstadoLiquidacionNomina.Contabilizada;
    }

    public static EstadoLiquidacionNomina Anular(EstadoLiquidacionNomina actual)
    {
        if (actual is EstadoLiquidacionNomina.Contabilizada or EstadoLiquidacionNomina.Anulada)
            throw new InvalidOperationException("No se puede anular una liquidacion contabilizada o ya anulada.");
        return EstadoLiquidacionNomina.Anulada;
    }

    public static void ValidarInmutable(EstadoLiquidacionNomina actual)
    {
        if (actual == EstadoLiquidacionNomina.Contabilizada)
            throw new InvalidOperationException("La liquidacion contabilizada es inmutable.");
    }
}
