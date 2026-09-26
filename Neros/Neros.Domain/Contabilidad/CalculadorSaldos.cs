namespace Neros.Domain.Contabilidad;

/// <summary>Saldos y balance de comprobacion (plan §34).</summary>
public static class CalculadorSaldos
{
    public static decimal SaldoFirmado(IEnumerable<(LadoMovimiento Lado, decimal Importe)> movimientos)
    {
        var debitos = movimientos.Where(m => m.Lado == LadoMovimiento.Debito).Sum(m => m.Importe);
        var creditos = movimientos.Where(m => m.Lado == LadoMovimiento.Credito).Sum(m => m.Importe);
        return debitos - creditos;
    }

    public static decimal SaldoPresentacion(decimal saldoAnterior, NaturalezaCuenta naturaleza, decimal debitos, decimal creditos) =>
        naturaleza == NaturalezaCuenta.Debito
            ? saldoAnterior + debitos - creditos
            : saldoAnterior + creditos - debitos;

    public static SaldoCuenta CalcularSaldoCuenta(
        CuentaContable cuenta, decimal saldoAnterior, IEnumerable<(LadoMovimiento Lado, decimal Importe)> movimientosPeriodo)
    {
        var lista = movimientosPeriodo.ToList();
        var debitos = lista.Where(m => m.Lado == LadoMovimiento.Debito).Sum(m => m.Importe);
        var creditos = lista.Where(m => m.Lado == LadoMovimiento.Credito).Sum(m => m.Importe);
        var firmado = SaldoFirmado(lista) + saldoAnterior;
        var presentacion = SaldoPresentacion(saldoAnterior, cuenta.Naturaleza, debitos, creditos);
        return new SaldoCuenta(cuenta.Id, firmado, presentacion);
    }

    public static BalanceComprobacionResultado BalanceComprobacion(
        IReadOnlyDictionary<Guid, CuentaContable> cuentas,
        IReadOnlyList<MovimientoLinea> movimientos,
        IReadOnlyDictionary<Guid, decimal> saldosAnteriores)
    {
        var totalDebitos = movimientos.Where(m => m.Lado == LadoMovimiento.Debito).Sum(m => m.ImporteMonedaFuncional);
        var totalCreditos = movimientos.Where(m => m.Lado == LadoMovimiento.Credito).Sum(m => m.ImporteMonedaFuncional);
        decimal totalSaldosDebito = 0, totalSaldosCredito = 0;
        foreach (var cuenta in cuentas.Values.Where(c => !TiposContables.EsCuentaOrden(c.Tipo)))
        {
            var movs = movimientos.Where(m => m.CuentaId == cuenta.Id);
            var saldo = CalcularSaldoCuenta(cuenta, saldosAnteriores.GetValueOrDefault(cuenta.Id), movs.Select(m => (m.Lado, m.ImporteMonedaFuncional)));
            if (cuenta.Naturaleza == NaturalezaCuenta.Debito && saldo.SaldoPresentacion >= 0)
                totalSaldosDebito += saldo.SaldoPresentacion;
            else if (cuenta.Naturaleza == NaturalezaCuenta.Credito && saldo.SaldoPresentacion >= 0)
                totalSaldosCredito += saldo.SaldoPresentacion;
            else if (cuenta.Naturaleza == NaturalezaCuenta.Debito)
                totalSaldosCredito += Math.Abs(saldo.SaldoPresentacion);
            else
                totalSaldosDebito += Math.Abs(saldo.SaldoPresentacion);
        }
        var cuadrado = totalDebitos == totalCreditos && totalSaldosDebito == totalSaldosCredito;
        return new BalanceComprobacionResultado(totalDebitos, totalCreditos, totalSaldosDebito, totalSaldosCredito, cuadrado);
    }
}
