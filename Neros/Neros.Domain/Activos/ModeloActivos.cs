namespace Neros.Domain.Activos;

public enum EstadoActivoFijo
{
    Borrador,
    Activo,
    DadoDeBaja
}

public enum MetodoDepreciacionActivo
{
    Lineal
}

public enum TipoEventoActivo
{
    Adquisicion,
    Depreciacion,
    Deterioro,
    Baja
}

public enum EstadoDepreciacionPeriodo
{
    Calculada,
    Contabilizada
}

public sealed record ActivoFijoParametros(
    Guid Id,
    decimal CostoAdquisicion,
    decimal ValorResidual,
    int VidaUtilMeses,
    MetodoDepreciacionActivo Metodo,
    EstadoActivoFijo Estado,
    decimal DepreciacionAcumulada,
    decimal DeterioroAcumulado);

public sealed record PeriodoDepreciacion(int Anio, int Mes);

public sealed record ResultadoCuotaDepreciacion(
    decimal ImportePeriodo,
    decimal DepreciacionAcumulada,
    decimal ValorEnLibros,
    bool UltimoPeriodo);

public sealed record ResultadoDeterioro(
    decimal ImporteDeterioro,
    decimal DeterioroAcumulado,
    decimal ValorEnLibros);
