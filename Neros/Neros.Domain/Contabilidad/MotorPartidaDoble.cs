using Neros.Domain.Globalizacion;

namespace Neros.Domain.Contabilidad;

/// <summary>Validacion de partida doble y contabilizacion (plan §33).</summary>
public static class MotorPartidaDoble
{
    public static void ValidarMovimientos(IReadOnlyList<MovimientoLinea> movimientos, IReadOnlyDictionary<Guid, CuentaContable> cuentas)
    {
        ArgumentNullException.ThrowIfNull(movimientos);
        ArgumentNullException.ThrowIfNull(cuentas);
        if (movimientos.Count < 2)
            throw new InvalidOperationException("Un comprobante requiere al menos dos movimientos.");
        foreach (var movimiento in movimientos)
        {
            if (movimiento.ImporteMonedaFuncional <= 0)
                throw new InvalidOperationException("El importe funcional debe ser positivo.");
            if (!cuentas.TryGetValue(movimiento.CuentaId, out var cuenta))
                throw new InvalidOperationException("Cuenta contable desconocida.");
            if (!cuenta.AdmiteMovimiento)
                throw new InvalidOperationException($"La cuenta {cuenta.Codigo} no admite movimiento.");
        }
        var debitos = movimientos.Where(m => m.Lado == LadoMovimiento.Debito).Sum(m => m.ImporteMonedaFuncional);
        var creditos = movimientos.Where(m => m.Lado == LadoMovimiento.Credito).Sum(m => m.ImporteMonedaFuncional);
        if (debitos != creditos)
            throw new InvalidOperationException($"Comprobante descuadrado: debitos {debitos} != creditos {creditos}.");
    }

    public static ComprobanteContable Contabilizar(
        ComprobanteContable borrador, IReadOnlyDictionary<Guid, CuentaContable> cuentas, PeriodoContable periodo, PoliticaRedondeo politica)
    {
        if (borrador.Estado != EstadoComprobante.Borrador)
            throw new InvalidOperationException("Solo se contabilizan comprobantes en borrador.");
        if (borrador.Fecha < periodo.FechaInicio || borrador.Fecha > periodo.FechaFin)
            throw new InvalidOperationException("La fecha del comprobante cae fuera del periodo.");
        if (periodo.Estado is EstadoPeriodoContable.Cerrado or EstadoPeriodoContable.EnCierre)
            throw new InvalidOperationException("El periodo no admite nuevos movimientos.");
        var movimientos = borrador.Movimientos.Select(m => m with
        {
            ImporteMonedaFuncional = MotorRedondeo.Aplicar(m.ImporteMonedaFuncional, politica.Validada())
        }).ToList();
        ValidarMovimientos(movimientos, cuentas);
        return borrador with { Estado = EstadoComprobante.Contabilizado, Movimientos = movimientos };
    }
}
