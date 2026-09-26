namespace Neros.Domain.Cartera;

/// <summary>Aplicaciones de pago y anticipo (plan §43).</summary>
public static class GestorAplicacionPago
{
    public static ResultadoRegistroPago Registrar(PagoCarteraEntrada pago, IReadOnlyList<SaldoDocumentoCartera> saldosActuales)
    {
        ArgumentNullException.ThrowIfNull(pago);
        if (pago.ImportePago <= 0) throw new ArgumentOutOfRangeException(nameof(pago));
        if (pago.Aplicaciones.Count == 0)
            throw new ArgumentException("Se requiere al menos una aplicacion o registrar el pago completo como anticipo.", nameof(pago));

        var aplicado = 0m;
        var movimientos = new List<MovimientoCarteraEntrada>();
        var saldos = saldosActuales.ToDictionary(s => s.DocumentoId);

        foreach (var aplicacion in pago.Aplicaciones)
        {
            if (aplicacion.ImporteAplicado <= 0) throw new ArgumentOutOfRangeException(nameof(pago));
            if (!saldos.TryGetValue(aplicacion.DocumentoId, out var saldoDoc))
                throw new InvalidOperationException($"Documento {aplicacion.DocumentoId} sin saldo conocido.");
            if (aplicacion.ImporteAplicado > saldoDoc.Saldo)
                throw new InvalidOperationException($"La aplicacion excede el saldo del documento {aplicacion.DocumentoId}.");
            aplicado += aplicacion.ImporteAplicado;
            movimientos.Add(new MovimientoCarteraEntrada(
                Guid.NewGuid(),
                pago.TipoCartera,
                pago.TerceroId,
                aplicacion.DocumentoId,
                NaturalezaMovimientoCartera.Abono,
                ConceptoMovimientoCartera.PagoAplicado,
                aplicacion.ImporteAplicado,
                pago.FechaPago));
            saldos[aplicacion.DocumentoId] = saldoDoc with { Saldo = saldoDoc.Saldo - aplicacion.ImporteAplicado };
        }

        var anticipo = pago.ImportePago - aplicado;
        if (anticipo < 0)
            throw new InvalidOperationException("La suma aplicada supera el importe del pago.");
        if (anticipo > 0)
        {
            movimientos.Add(new MovimientoCarteraEntrada(
                Guid.NewGuid(),
                pago.TipoCartera,
                pago.TerceroId,
                null,
                NaturalezaMovimientoCartera.Abono,
                ConceptoMovimientoCartera.Anticipo,
                anticipo,
                pago.FechaPago));
        }

        ValidarConservacionImporte(pago.ImportePago, aplicado, anticipo);
        return new ResultadoRegistroPago(pago, anticipo, movimientos);
    }

    public static void ValidarConservacionImporte(decimal importePago, decimal aplicado, decimal anticipo)
    {
        if (importePago != aplicado + anticipo)
            throw new InvalidOperationException(
                $"Importe de pago inconsistente: {importePago} != {aplicado} + {anticipo}.");
    }
}
