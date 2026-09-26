namespace Neros.Domain.Cartera;

public enum TipoCartera
{
    CxC,
    CxP
}

public enum NaturalezaMovimientoCartera
{
    Cargo,
    Abono
}

public enum ConceptoMovimientoCartera
{
    Factura,
    NotaDebito,
    Interes,
    PagoAplicado,
    NotaCredito,
    Retencion,
    Castigo,
    Anticipo
}

public sealed record MovimientoCarteraEntrada(
    Guid Id,
    TipoCartera TipoCartera,
    Guid TerceroId,
    Guid? DocumentoId,
    NaturalezaMovimientoCartera Naturaleza,
    ConceptoMovimientoCartera Concepto,
    decimal Importe,
    DateOnly FechaMovimiento);

public sealed record CuotaDocumentoEntrada(int NumeroCuota, DateOnly FechaVencimiento, decimal ImporteCuota);

public sealed record DocumentoCarteraEntrada(
    Guid DocumentoId,
    TipoCartera TipoCartera,
    Guid TerceroId,
    string Numero,
    DateOnly FechaDocumento,
    decimal TotalDocumento,
    IReadOnlyList<CuotaDocumentoEntrada> Cuotas);

public sealed record AplicacionPagoEntrada(Guid DocumentoId, decimal ImporteAplicado, Guid? CuotaId = null);

public sealed record PagoCarteraEntrada(
    Guid PagoId,
    TipoCartera TipoCartera,
    Guid TerceroId,
    decimal ImportePago,
    DateOnly FechaPago,
    IReadOnlyList<AplicacionPagoEntrada> Aplicaciones);

public sealed record ResultadoRegistroPago(
    PagoCarteraEntrada Pago,
    decimal ImporteAnticipo,
    IReadOnlyList<MovimientoCarteraEntrada> MovimientosGenerados);

public sealed record SaldoDocumentoCartera(Guid DocumentoId, decimal Saldo);

public sealed record SaldoTerceroCartera(Guid TerceroId, TipoCartera TipoCartera, decimal Saldo);
