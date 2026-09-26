namespace Neros.Domain.Tesoreria;

/// <summary>Saldo de cuenta desde movimientos (plan §44).</summary>
public static class CalculadorSaldoTesoreria
{
    public static decimal ImpactoSaldo(TipoMovimientoTesoreria tipo, decimal importe)
    {
        if (importe <= 0) throw new ArgumentOutOfRangeException(nameof(importe));
        return tipo is TipoMovimientoTesoreria.Ingreso or TipoMovimientoTesoreria.TransferenciaEntrada
            ? importe
            : -importe;
    }

    public static decimal Saldo(decimal saldoInicial, IReadOnlyList<MovimientoTesoreriaEntrada> movimientos, Guid? cuentaId = null)
    {
        var lista = cuentaId is null
            ? movimientos
            : movimientos.Where(m => m.CuentaId == cuentaId).ToList();
        return saldoInicial + lista.Sum(m => ImpactoSaldo(m.Tipo, m.Importe));
    }
}
