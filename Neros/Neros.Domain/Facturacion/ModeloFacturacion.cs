namespace Neros.Domain.Facturacion;

public enum EstadoDocumentoComercial
{
    Borrador,
    Emitido,
    Anulado
}

public enum TipoDocumentoFiscal
{
    FacturaVenta,
    NotaCredito,
    NotaDebito
}

public enum EstadoNumeracionFiscal
{
    Activo,
    Agotado,
    Vencido,
    Inactivo
}

public enum AmbienteDian
{
    Habilitacion,
    Produccion
}

/// <summary>Estados del documento electronico (plan §50).</summary>
public enum EstadoDocumentoElectronico
{
    Borrador,
    Generado,
    Firmado,
    Enviado,
    Validado,
    Rechazado,
    Entregado,
    Anulado
}

public sealed record RangoNumeracionFiscal(
    Guid Id,
    TipoDocumentoFiscal TipoDocumento,
    string Prefijo,
    long Desde,
    long Hasta,
    long Actual,
    DateOnly VigenciaDesde,
    DateOnly VigenciaHasta,
    string Resolucion,
    EstadoNumeracionFiscal Estado);

public sealed record DatosEmisorSnapshot(
    string Nit,
    string RazonSocial,
    string? ResponsabilidadesFiscales,
    string VersionNormativa,
    string VersionReglasTributarias,
    string VersionSoftware);

public sealed record DatosAdquirenteSnapshot(
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string RazonSocial,
    string? Direccion);

public sealed record SnapshotLegalDocumentoFiscal(
    TipoDocumentoFiscal TipoDocumento,
    string Prefijo,
    long NumeroFiscal,
    string NumeroPresentacion,
    string Resolucion,
    DateOnly FechaExpedicionNegocio,
    DateTimeOffset ExpedidoEnUtc,
    string MonedaCodigo,
    decimal TasaCambio,
    DatosEmisorSnapshot Emisor,
    DatosAdquirenteSnapshot Adquirente,
    Guid? ImpuestosVersionPublicadaId,
    int? ImpuestosVersionNumero,
    decimal Subtotal,
    decimal TotalImpuestos,
    decimal TotalRetenciones,
    decimal Total);

public sealed record ResultadoReservaNumero(long NumeroAsignado, string NumeroPresentacion, EstadoNumeracionFiscal EstadoRango);

public sealed record ResultadoIdempotencia(bool EsRepeticion, string? RespuestaSerializada);
