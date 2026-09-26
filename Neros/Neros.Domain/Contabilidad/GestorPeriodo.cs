namespace Neros.Domain.Contabilidad;

/// <summary>Transiciones de periodo y reapertura (plan §35).</summary>
public static class GestorPeriodo
{
    public static PeriodoContable IniciarCierre(PeriodoContable periodo)
    {
        if (periodo.Estado != EstadoPeriodoContable.Abierto && periodo.Estado != EstadoPeriodoContable.Reabierto)
            throw new InvalidOperationException("Solo un periodo abierto o reabierto puede entrar en cierre.");
        return periodo with { Estado = EstadoPeriodoContable.EnCierre };
    }

    public static PeriodoContable Cerrar(PeriodoContable periodo, BalanceComprobacionResultado balance, bool sinDescuadres)
    {
        if (periodo.Estado != EstadoPeriodoContable.EnCierre)
            throw new InvalidOperationException("El periodo debe estar en cierre.");
        if (!sinDescuadres || !balance.Cuadrado)
            throw new InvalidOperationException("No se puede cerrar con comprobantes descuadrados o errores de integridad.");
        return periodo with { Estado = EstadoPeriodoContable.Cerrado };
    }

    public static PeriodoContable Reabrir(PeriodoContable periodo, string motivo)
    {
        if (periodo.Estado != EstadoPeriodoContable.Cerrado)
            throw new InvalidOperationException("Solo un periodo cerrado puede reabrirse.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("La reapertura exige motivo.", nameof(motivo));
        return periodo with { Estado = EstadoPeriodoContable.Reabierto };
    }
}
