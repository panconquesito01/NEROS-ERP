namespace Neros.Domain.Produccion;

public static class MotorMovimientosProduccion
{
    public static SaldosOrdenProduccion AplicarMovimiento(
        SaldosOrdenProduccion saldos,
        EstadoOrdenProduccion estado,
        TipoMovimientoProduccion tipo,
        decimal cantidad)
    {
        MaquinaEstadosOrdenProduccion.ValidarMovimiento(estado, tipo);
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));

        return tipo switch
        {
            TipoMovimientoProduccion.Consumo => saldos with
            {
                CantidadConsumidaComponente = saldos.CantidadConsumidaComponente + cantidad
            },
            TipoMovimientoProduccion.Devolucion => saldos with
            {
                CantidadDevueltaComponente = saldos.CantidadDevueltaComponente + cantidad
            },
            TipoMovimientoProduccion.Terminado => RegistrarTerminado(saldos, cantidad),
            TipoMovimientoProduccion.Merma => RegistrarMerma(saldos, cantidad),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };
    }

    private static SaldosOrdenProduccion RegistrarTerminado(SaldosOrdenProduccion saldos, decimal cantidad)
    {
        if (saldos.CantidadTerminada + saldos.CantidadMerma + cantidad > saldos.CantidadPlanificada)
            throw new InvalidOperationException("La cantidad terminada supera lo planificado.");
        return saldos with { CantidadTerminada = saldos.CantidadTerminada + cantidad };
    }

    private static SaldosOrdenProduccion RegistrarMerma(SaldosOrdenProduccion saldos, decimal cantidad)
    {
        if (saldos.CantidadTerminada + saldos.CantidadMerma + cantidad > saldos.CantidadPlanificada)
            throw new InvalidOperationException("La merma supera lo planificado.");
        return saldos with { CantidadMerma = saldos.CantidadMerma + cantidad };
    }

    public static decimal ConsumoNetoComponente(SaldosOrdenProduccion saldos) =>
        saldos.CantidadConsumidaComponente - saldos.CantidadDevueltaComponente;
}
