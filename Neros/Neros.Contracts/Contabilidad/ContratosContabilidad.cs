namespace Neros.Contracts.Contabilidad;

public sealed record ComprobanteOrigenDto(
    string ModuloOrigen, string TipoDocumentoOrigen, Guid DocumentoOrigenId, string NumeroDocumentoOrigen);

public static class CodigosEventoContabilizacion
{
    public const string VentaFacturada = "VENTA_FACTURADA";
    public const string CompraRecibida = "COMPRA_RECIBIDA";
}
