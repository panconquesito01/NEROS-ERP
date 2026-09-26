namespace Neros.Domain.Facturacion;

/// <summary>Emision comercial → reserva fiscal + inicio electronico.</summary>
public static class MotorEmisionComercial
{
    public static (SnapshotLegalDocumentoFiscal Snapshot, EstadoDocumentoElectronico EstadoElectronicoInicial)
        Emitir(
            EstadoDocumentoComercial estadoComercial,
            RangoNumeracionFiscal rango,
            DateOnly fechaNegocio,
            DateTimeOffset ahoraUtc,
            string monedaCodigo,
            decimal tasaCambio,
            DatosEmisorSnapshot emisor,
            DatosAdquirenteSnapshot adquirente,
            Guid? impuestosVersionPublicadaId,
            int? impuestosVersionNumero,
            decimal subtotal,
            decimal totalImpuestos,
            decimal totalRetenciones,
            decimal total,
            TipoDocumentoFiscal tipo = TipoDocumentoFiscal.FacturaVenta)
    {
        if (estadoComercial != EstadoDocumentoComercial.Borrador)
            throw new InvalidOperationException("Solo un documento comercial en borrador puede emitirse.");
        if (impuestosVersionPublicadaId is null || impuestosVersionNumero is null)
            throw new InvalidOperationException("Se requiere snapshot de impuestos versionado para emitir.");
        if (string.IsNullOrWhiteSpace(adquirente.RazonSocial))
            throw new InvalidOperationException("Se requiere snapshot del adquirente.");

        var reserva = MotorNumeracionFiscal.ReservarSiguiente(rango, fechaNegocio);
        var snapshot = MotorSnapshotLegal.Crear(
            tipo, rango, reserva.NumeroAsignado, reserva.NumeroPresentacion,
            fechaNegocio, ahoraUtc, monedaCodigo, tasaCambio, emisor, adquirente,
            impuestosVersionPublicadaId, impuestosVersionNumero,
            subtotal, totalImpuestos, totalRetenciones, total);

        return (snapshot, EstadoDocumentoElectronico.Borrador);
    }
}
