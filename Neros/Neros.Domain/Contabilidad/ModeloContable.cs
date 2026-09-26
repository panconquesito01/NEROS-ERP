namespace Neros.Domain.Contabilidad;

public sealed record CuentaContable(
    Guid Id, string Codigo, NaturalezaCuenta Naturaleza, TipoCuenta Tipo, bool AdmiteMovimiento);

public sealed record PeriodoContable(Guid Id, DateOnly FechaInicio, DateOnly FechaFin, EstadoPeriodoContable Estado);

public sealed record MovimientoLinea(
    Guid CuentaId, LadoMovimiento Lado, decimal ImporteMonedaFuncional, Guid? TerceroId = null);

public sealed record ComprobanteContable(
    Guid Id, Guid PeriodoId, DateOnly Fecha, EstadoComprobante Estado,
    IReadOnlyList<MovimientoLinea> Movimientos,
    Guid? ComprobanteOriginalId = null,
    Guid? ComprobanteReversionId = null,
    string? MotivoReversion = null,
    OrigenDocumentoContable? Origen = null,
    int? ReglaContabilizacionVersion = null);

public sealed record OrigenDocumentoContable(
    string ModuloOrigen, string TipoDocumentoOrigen, Guid DocumentoOrigenId, string NumeroDocumentoOrigen);

public sealed record SaldoCuenta(Guid CuentaId, decimal SaldoFirmado, decimal SaldoPresentacion);

public sealed record BalanceComprobacionResultado(
    decimal TotalDebitos, decimal TotalCreditos, decimal TotalSaldosDebito, decimal TotalSaldosCredito, bool Cuadrado);

public sealed record EcuacionContableResultado(
    decimal Activo, decimal Pasivo, decimal Patrimonio, decimal Ingresos, decimal Costos, decimal Gastos, bool Cumple);
