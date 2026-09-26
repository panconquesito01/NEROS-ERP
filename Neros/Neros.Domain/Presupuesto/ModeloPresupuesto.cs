namespace Neros.Domain.Presupuesto;

public enum EstadoVersionPresupuesto
{
    Borrador,
    Aprobada,
    Cerrada
}

public sealed record CeldaPresupuesto(decimal ValorPresupuestado, decimal ValorReal);

public sealed record ResultadoIndicadoresPresupuesto(
    decimal Variacion,
    decimal? EjecucionPorcentaje,
    bool EjecucionNoAplica);

public sealed record ResultadoConsolidadoPresupuesto(
    decimal TotalPresupuestado,
    decimal TotalReal,
    ResultadoIndicadoresPresupuesto Indicadores);
