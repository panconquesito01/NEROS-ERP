namespace Neros.Domain.Inventario;

/// <summary>Kardex reconstruido desde movimientos procesados (plan §42).</summary>
public static class ReconstructorKardex
{
    public static IReadOnlyList<LineaKardex> ConstruirDesdePendientes(
        IReadOnlyList<MovimientoInventarioPendiente> pendientes, PoliticaBodega politica)
    {
        var (_, procesados) = RecosteadorInventario.Recostear(pendientes, politica);
        return ConstruirDesdeRecosteo(procesados, pendientes);
    }

    public static IReadOnlyList<LineaKardex> ConstruirDesdeRecosteo(
        IReadOnlyList<MovimientoInventarioProcesado> procesados, IReadOnlyList<MovimientoInventarioPendiente> pendientes)
    {
        var esEntrada = pendientes.ToDictionary(p => p.Id, p => p.EsEntrada);
        var lineas = new List<LineaKardex>();
        decimal saldoCantidad = 0, saldoValor = 0;
        foreach (var movimiento in procesados.OrderBy(m => m.Fecha).ThenBy(m => m.Secuencia))
        {
            if (esEntrada[movimiento.Id])
            {
                saldoCantidad += movimiento.Cantidad;
                saldoValor += movimiento.ValorTotal;
                var costo = saldoCantidad > 0 ? saldoValor / saldoCantidad : movimiento.CostoUnitarioAplicado;
                lineas.Add(new LineaKardex(movimiento.Fecha, movimiento.Secuencia, movimiento.Tipo,
                    movimiento.Cantidad, movimiento.ValorTotal, null, null, saldoCantidad, saldoValor, costo));
            }
            else
            {
                saldoCantidad -= movimiento.Cantidad;
                saldoValor -= movimiento.ValorTotal;
                lineas.Add(new LineaKardex(movimiento.Fecha, movimiento.Secuencia, movimiento.Tipo,
                    null, null, movimiento.Cantidad, movimiento.ValorTotal, saldoCantidad, saldoValor,
                    movimiento.CostoUnitarioAplicado));
            }
        }
        return lineas;
    }
}
