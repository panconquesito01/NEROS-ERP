namespace Neros.Domain.Inventario;

/// <summary>Recalculo cronologico tras movimientos retroactivos (plan §49).</summary>
public static class RecosteadorInventario
{
    public static (EstadoExistencia EstadoFinal, IReadOnlyList<MovimientoInventarioProcesado> Procesados) Recostear(
        IEnumerable<MovimientoInventarioPendiente> movimientos, PoliticaBodega politica,
        IEnumerable<PeriodoInventario>? periodos = null)
    {
        var ordenados = movimientos.OrderBy(m => m.Fecha).ThenBy(m => m.Secuencia).ToList();
        ValidarPeriodos(ordenados, periodos);
        var estado = new EstadoExistencia(0, 0, 0);
        var procesados = new List<MovimientoInventarioProcesado>();
        foreach (var movimiento in ordenados)
        {
            (estado, var procesado) = MotorPromedioPonderado.Aplicar(estado, movimiento, politica);
            procesados.Add(procesado);
        }
        return (estado, procesados);
    }

    public static void ValidarPeriodos(IReadOnlyList<MovimientoInventarioPendiente> movimientos, IEnumerable<PeriodoInventario>? periodos)
    {
        if (periodos is null) return;
        foreach (var movimiento in movimientos)
        {
            foreach (var periodo in periodos.Where(p => p.Estado == EstadoPeriodoInventario.Cerrado))
            {
                if (movimiento.Fecha >= periodo.FechaInicio && movimiento.Fecha <= periodo.FechaFin)
                    throw new InvalidOperationException("No se permiten movimientos en un periodo de inventario cerrado.");
            }
        }
    }
}
