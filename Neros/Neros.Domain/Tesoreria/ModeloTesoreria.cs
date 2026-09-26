namespace Neros.Domain.Tesoreria;

public enum TipoCuentaTesoreria
{
    Caja,
    Banco
}

public enum TipoMovimientoTesoreria
{
    Ingreso,
    Egreso,
    TransferenciaEntrada,
    TransferenciaSalida
}

public enum EstadoConciliacion
{
    Borrador,
    Cerrada
}

public sealed record MovimientoTesoreriaEntrada(
    Guid Id,
    Guid CuentaId,
    TipoMovimientoTesoreria Tipo,
    decimal Importe,
    DateOnly FechaMovimiento,
    Guid? CarteraPagoId = null);

public sealed record TransferenciaTesoreriaEntrada(
    Guid TransferenciaId,
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Importe,
    DateOnly Fecha);

public sealed record ResultadoTransferenciaTesoreria(
    TransferenciaTesoreriaEntrada Transferencia,
    MovimientoTesoreriaEntrada Salida,
    MovimientoTesoreriaEntrada Entrada);

public sealed record LineaExtractoEntrada(Guid Id, DateOnly Fecha, decimal Importe, bool EsCredito);

public sealed record ExtractoBancarioEntrada(
    Guid ExtractoId,
    Guid CuentaId,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    decimal SaldoInicialExtracto,
    decimal SaldoFinalExtracto,
    IReadOnlyList<LineaExtractoEntrada> Lineas);

public sealed record ParConciliacionEntrada(Guid? MovimientoId, Guid? LineaExtractoId, decimal Importe);

public sealed record ConciliacionBancariaEntrada(
    Guid ConciliacionId,
    Guid CuentaId,
    ExtractoBancarioEntrada Extracto,
    decimal SaldoLibroInicial,
    IReadOnlyList<MovimientoTesoreriaEntrada> MovimientosLibro,
    IReadOnlyList<ParConciliacionEntrada> Enlaces);

public sealed record ResultadoConciliacionBancaria(
    decimal SaldoLibroFinal,
    decimal SaldoExtractoFinal,
    decimal Diferencia,
    IReadOnlyList<MovimientoTesoreriaEntrada> MovimientosSinConciliar,
    IReadOnlyList<LineaExtractoEntrada> LineasExtractoSinConciliar);
