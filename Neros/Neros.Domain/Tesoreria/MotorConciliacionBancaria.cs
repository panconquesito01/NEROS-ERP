namespace Neros.Domain.Tesoreria;

/// <summary>Conciliacion bancaria: saldo libro vs extracto (criterio fase 15).</summary>
public static class MotorConciliacionBancaria
{
    public static ResultadoConciliacionBancaria Conciliar(ConciliacionBancariaEntrada entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        if (entrada.Extracto.CuentaId != entrada.CuentaId)
            throw new InvalidOperationException("El extracto pertenece a otra cuenta.");
        ValidarEnlaces(entrada.Enlaces, entrada.MovimientosLibro, entrada.Extracto.Lineas);

        var movConciliados = entrada.Enlaces.Where(e => e.MovimientoId is not null).Select(e => e.MovimientoId!.Value).ToHashSet();
        var lineasConciliadas = entrada.Enlaces.Where(e => e.LineaExtractoId is not null).Select(e => e.LineaExtractoId!.Value).ToHashSet();

        var saldoLibroFinal = CalculadorSaldoTesoreria.Saldo(
            entrada.SaldoLibroInicial, entrada.MovimientosLibro, entrada.CuentaId);
        var saldoExtractoFinal = entrada.Extracto.SaldoFinalExtracto;
        var diferencia = saldoExtractoFinal - saldoLibroFinal;

        var movimientosPendientes = entrada.MovimientosLibro
            .Where(m => m.CuentaId == entrada.CuentaId && !movConciliados.Contains(m.Id))
            .ToList();
        var lineasPendientes = entrada.Extracto.Lineas
            .Where(l => !lineasConciliadas.Contains(l.Id))
            .ToList();

        return new ResultadoConciliacionBancaria(
            saldoLibroFinal, saldoExtractoFinal, diferencia, movimientosPendientes, lineasPendientes);
    }

    public static void ValidarCierre(EstadoConciliacion estado, decimal diferencia)
    {
        if (estado == EstadoConciliacion.Cerrada && diferencia != 0)
            throw new InvalidOperationException("No se puede cerrar una conciliacion con diferencia distinta de cero.");
    }

    public static decimal SaldoExtractoDesdeLineas(decimal saldoInicial, IReadOnlyList<LineaExtractoEntrada> lineas)
        => saldoInicial + lineas.Sum(l => l.EsCredito ? l.Importe : -l.Importe);

    private static void ValidarEnlaces(
        IReadOnlyList<ParConciliacionEntrada> enlaces,
        IReadOnlyList<MovimientoTesoreriaEntrada> movimientos,
        IReadOnlyList<LineaExtractoEntrada> lineas)
    {
        var movimientosPorId = movimientos.ToDictionary(m => m.Id);
        var lineasPorId = lineas.ToDictionary(l => l.Id);
        foreach (var enlace in enlaces)
        {
            if (enlace.Importe <= 0) throw new ArgumentOutOfRangeException(nameof(enlaces));
            if (enlace.MovimientoId is null && enlace.LineaExtractoId is null)
                throw new ArgumentException("Enlace vacio.");
            if (enlace.MovimientoId is not null && !movimientosPorId.ContainsKey(enlace.MovimientoId.Value))
                throw new InvalidOperationException("Movimiento de enlace desconocido.");
            if (enlace.LineaExtractoId is not null && !lineasPorId.ContainsKey(enlace.LineaExtractoId.Value))
                throw new InvalidOperationException("Linea de extracto desconocida.");
            if (enlace.MovimientoId is not null && enlace.LineaExtractoId is not null)
            {
                var mov = movimientosPorId[enlace.MovimientoId.Value];
                var lin = lineasPorId[enlace.LineaExtractoId.Value];
                var impacto = Math.Abs(CalculadorSaldoTesoreria.ImpactoSaldo(mov.Tipo, mov.Importe));
                var impactoExtracto = lin.Importe;
                if (impacto != enlace.Importe || impactoExtracto != enlace.Importe)
                    throw new InvalidOperationException("El par conciliado no cuadra en importe.");
            }
        }
    }
}
