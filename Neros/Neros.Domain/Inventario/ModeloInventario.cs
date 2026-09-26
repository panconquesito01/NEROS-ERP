namespace Neros.Domain.Inventario;

public enum TipoMovimientoInventario { Entrada, Salida, Ajuste, Transferencia, Devolucion, Produccion }

public enum EstadoPeriodoInventario { Abierto, Cerrado }

public sealed record ClaveExistencia(Guid ProductoId, Guid BodegaId);

public sealed record PeriodoInventario(Guid Id, DateOnly FechaInicio, DateOnly FechaFin, EstadoPeriodoInventario Estado);

public sealed record PoliticaBodega(bool PermitirExistenciasNegativas);

public sealed record MovimientoInventarioPendiente(
    Guid Id, DateOnly Fecha, int Secuencia, TipoMovimientoInventario Tipo, bool EsEntrada, decimal Cantidad,
    decimal? CostoUnitarioEntrada);

public sealed record MovimientoInventarioProcesado(
    Guid Id, DateOnly Fecha, int Secuencia, TipoMovimientoInventario Tipo, decimal Cantidad,
    decimal CostoUnitarioAplicado, decimal ValorTotal, bool MarcadoAjusteNegativo);

public sealed record EstadoExistencia(decimal Cantidad, decimal ValorTotal, decimal CostoPromedio);

public sealed record LineaKardex(
    DateOnly Fecha, int Secuencia, TipoMovimientoInventario Tipo,
    decimal? CantidadEntrada, decimal? ValorEntrada, decimal? CantidadSalida, decimal? ValorSalida,
    decimal SaldoCantidad, decimal SaldoValor, decimal CostoUnitario);
