namespace Neros.Domain.Contabilidad;

/// <summary>Reversion trazable de comprobantes (plan §35).</summary>
public static class ReversionComprobante
{
    public static ComprobanteContable CrearReversion(ComprobanteContable original, Guid nuevoId, string motivo)
    {
        if (original.Estado != EstadoComprobante.Contabilizado)
            throw new InvalidOperationException("Solo se revierte un comprobante contabilizado.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo de reversion obligatorio.", nameof(motivo));
        var movimientosInversos = original.Movimientos.Select(m => m with
        {
            Lado = m.Lado == LadoMovimiento.Debito ? LadoMovimiento.Credito : LadoMovimiento.Debito
        }).ToList();
        return new ComprobanteContable(
            nuevoId, original.PeriodoId, original.Fecha, EstadoComprobante.Contabilizado, movimientosInversos,
            original.Id, null, motivo, original.Origen, original.ReglaContabilizacionVersion);
    }

    public static ComprobanteContable MarcarOriginalReversado(ComprobanteContable original, Guid comprobanteReversionId) =>
        original with
        {
            Estado = EstadoComprobante.Reversado,
            ComprobanteReversionId = comprobanteReversionId
        };
}
