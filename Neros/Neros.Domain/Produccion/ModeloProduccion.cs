namespace Neros.Domain.Produccion;

public enum EstadoOrdenProduccion
{
    Planificada,
    Liberada,
    EnProceso,
    Terminada,
    Cerrada,
    Anulada
}

public enum TipoMovimientoProduccion
{
    Consumo,
    Devolucion,
    Terminado,
    Merma
}

public sealed record LineaListaMateriales(Guid ComponenteReferenciaId, decimal CantidadPorBase);

public sealed record ListaMaterialesDefinicion(
    Guid Id,
    decimal CantidadBaseSalida,
    IReadOnlyList<LineaListaMateriales> Lineas);

public sealed record RequerimientoMaterial(Guid ComponenteReferenciaId, decimal CantidadRequerida);

public sealed record EntradaCostoProduccion(
    decimal CostoMateriaPrima,
    decimal CostoManoObra,
    decimal CostoIndirecto,
    decimal CantidadTerminada);

public sealed record ResultadoCostoOrdenProduccion(
    decimal CostoMateriaPrima,
    decimal CostoManoObra,
    decimal CostoIndirecto,
    decimal CostoTotal,
    decimal CostoUnitarioTerminado);

public sealed record SaldosOrdenProduccion(
    decimal CantidadPlanificada,
    decimal CantidadTerminada,
    decimal CantidadMerma,
    decimal CantidadConsumidaComponente,
    decimal CantidadDevueltaComponente);
