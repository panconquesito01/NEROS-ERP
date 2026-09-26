namespace Neros.Domain.Facturacion;

/// <summary>Snapshot legal inmutable al emitir (plan §51).</summary>
public static class MotorSnapshotLegal
{
    public static SnapshotLegalDocumentoFiscal Crear(
        TipoDocumentoFiscal tipo,
        RangoNumeracionFiscal rango,
        long numeroAsignado,
        string numeroPresentacion,
        DateOnly fechaNegocio,
        DateTimeOffset expedidoEnUtc,
        string monedaCodigo,
        decimal tasaCambio,
        DatosEmisorSnapshot emisor,
        DatosAdquirenteSnapshot adquirente,
        Guid? impuestosVersionPublicadaId,
        int? impuestosVersionNumero,
        decimal subtotal,
        decimal totalImpuestos,
        decimal totalRetenciones,
        decimal total)
    {
        ArgumentNullException.ThrowIfNull(emisor);
        ArgumentNullException.ThrowIfNull(adquirente);
        if (numeroAsignado < rango.Desde || numeroAsignado > rango.Hasta)
            throw new ArgumentOutOfRangeException(nameof(numeroAsignado));
        if (string.IsNullOrWhiteSpace(emisor.Nit) || string.IsNullOrWhiteSpace(emisor.RazonSocial))
            throw new ArgumentException("Datos fiscales del emisor incompletos.");
        if (string.IsNullOrWhiteSpace(adquirente.NumeroIdentificacion) || string.IsNullOrWhiteSpace(adquirente.RazonSocial))
            throw new ArgumentException("Datos fiscales del adquirente incompletos.");
        if (tasaCambio <= 0) throw new ArgumentOutOfRangeException(nameof(tasaCambio));
        if (total < 0 || subtotal < 0) throw new ArgumentOutOfRangeException(nameof(total));

        return new SnapshotLegalDocumentoFiscal(
            tipo,
            rango.Prefijo.Trim(),
            numeroAsignado,
            numeroPresentacion.Trim(),
            rango.Resolucion.Trim(),
            fechaNegocio,
            expedidoEnUtc,
            monedaCodigo.Trim().ToUpperInvariant(),
            tasaCambio,
            emisor,
            adquirente,
            impuestosVersionPublicadaId,
            impuestosVersionNumero,
            subtotal,
            totalImpuestos,
            totalRetenciones,
            total);
    }
}
