namespace Neros.Domain.Proyectos;

public enum EstadoProyecto
{
    Planificado,
    Activo,
    Suspendido,
    Cerrado,
    Cancelado
}

public enum ConceptoPresupuestoProyecto
{
    Ingresos,
    Gastos,
    Compras,
    ManoObra,
    Otros
}

public enum ModuloOrigenProyecto
{
    Ventas,
    Compras,
    Contabilidad,
    Nomina,
    Produccion,
    Presupuesto,
    Tesoreria
}

public sealed record ProyectoContexto(
    Guid Id,
    EstadoProyecto Estado,
    decimal PresupuestoTotal,
    DateOnly FechaInicioPlan,
    DateOnly? FechaFinPlan);

public sealed record ReferenciaMovimientoProyectoEntrada(
    ModuloOrigenProyecto Modulo,
    string TipoDocumento,
    Guid DocumentoOrigenId,
    decimal ImporteAsignado,
    DateOnly FechaNegocio);

public sealed record ResultadoImputacionProyecto(
    decimal TotalImputado,
    decimal PresupuestoRestante,
    bool SuperaPresupuesto);

public sealed record ResultadoSeguimientoProyecto(
    decimal PresupuestoTotal,
    decimal TotalImputadoGastos,
    decimal VariacionPresupuesto,
    decimal? EjecucionPorcentaje,
    bool EjecucionNoAplica);
