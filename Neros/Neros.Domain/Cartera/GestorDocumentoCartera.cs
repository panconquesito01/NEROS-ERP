namespace Neros.Domain.Cartera;

public static class GestorDocumentoCartera
{
    public static MovimientoCarteraEntrada CargoInicialDocumento(DocumentoCarteraEntrada documento)
    {
        ArgumentNullException.ThrowIfNull(documento);
        if (documento.TotalDocumento <= 0) throw new ArgumentOutOfRangeException(nameof(documento));
        if (documento.Cuotas.Count > 0)
        {
            var sumaCuotas = documento.Cuotas.Sum(c => c.ImporteCuota);
            if (sumaCuotas != documento.TotalDocumento)
                throw new InvalidOperationException("La suma de cuotas debe igualar el total del documento.");
        }
        var concepto = documento.TipoCartera == TipoCartera.CxC
            ? ConceptoMovimientoCartera.Factura
            : ConceptoMovimientoCartera.Factura;
        return new MovimientoCarteraEntrada(
            Guid.NewGuid(),
            documento.TipoCartera,
            documento.TerceroId,
            documento.DocumentoId,
            NaturalezaMovimientoCartera.Cargo,
            concepto,
            documento.TotalDocumento,
            documento.FechaDocumento);
    }
}
