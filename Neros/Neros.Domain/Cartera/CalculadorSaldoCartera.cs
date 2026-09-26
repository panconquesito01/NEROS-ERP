namespace Neros.Domain.Cartera;

/// <summary>Saldo = cargos - abonos (plan §43).</summary>
public static class CalculadorSaldoCartera
{
    public static decimal SaldoDesdeMovimientos(IReadOnlyList<MovimientoCarteraEntrada> movimientos, Guid? documentoId = null)
    {
        var filtrados = documentoId is null
            ? movimientos
            : movimientos.Where(m => m.DocumentoId == documentoId).ToList();
        var cargos = filtrados.Where(m => m.Naturaleza == NaturalezaMovimientoCartera.Cargo).Sum(m => m.Importe);
        var abonos = filtrados.Where(m => m.Naturaleza == NaturalezaMovimientoCartera.Abono).Sum(m => m.Importe);
        return cargos - abonos;
    }

    public static SaldoDocumentoCartera SaldoDocumento(Guid documentoId, IReadOnlyList<MovimientoCarteraEntrada> movimientos)
        => new(documentoId, SaldoDesdeMovimientos(movimientos, documentoId));

    public static SaldoTerceroCartera SaldoTercero(
        Guid terceroId, TipoCartera tipo, IReadOnlyList<MovimientoCarteraEntrada> movimientos)
    {
        var delTercero = movimientos.Where(m => m.TerceroId == terceroId && m.TipoCartera == tipo).ToList();
        return new SaldoTerceroCartera(terceroId, tipo, SaldoDesdeMovimientos(delTercero));
    }
}
