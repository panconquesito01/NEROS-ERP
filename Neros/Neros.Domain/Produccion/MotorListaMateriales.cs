namespace Neros.Domain.Produccion;

public static class MotorListaMateriales
{
    public static IReadOnlyList<RequerimientoMaterial> Explosionar(
        ListaMaterialesDefinicion lista,
        decimal cantidadOrden)
    {
        ArgumentNullException.ThrowIfNull(lista);
        if (cantidadOrden <= 0) throw new ArgumentOutOfRangeException(nameof(cantidadOrden));
        if (lista.CantidadBaseSalida <= 0) throw new InvalidOperationException("Cantidad base de la lista invalida.");
        var factor = cantidadOrden / lista.CantidadBaseSalida;
        return lista.Lineas
            .GroupBy(l => l.ComponenteReferenciaId)
            .Select(g => new RequerimientoMaterial(g.Key, g.Sum(l => l.CantidadPorBase * factor)))
            .OrderBy(r => r.ComponenteReferenciaId)
            .ToList();
    }
}
