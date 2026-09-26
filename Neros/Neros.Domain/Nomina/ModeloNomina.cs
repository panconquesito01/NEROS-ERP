using Neros.Domain.Nomina.Legal;

namespace Neros.Domain.Nomina;

public enum EstadoEmpleado
{
    Activo,
    Retirado
}

public enum EstadoContrato
{
    Activo,
    Finalizado
}

public enum NaturalezaConceptoNomina
{
    Devengo,
    Deduccion
}

public enum TipoFormulaConceptoNomina
{
    Fijo,
    PorcentajeSalario,
    Horas,
    Manual
}

public enum EstadoPeriodoNomina
{
    Abierto,
    Cerrado
}

public enum EstadoLiquidacionNomina
{
    Borrador,
    Calculada,
    Contabilizada,
    Anulada
}

public sealed record ContratoNomina(
    Guid Id,
    Guid EmpleadoId,
    DateOnly FechaInicio,
    DateOnly? FechaFin,
    string TipoContrato,
    decimal SalarioBase,
    decimal HorasSemanales,
    EstadoContrato Estado);

public sealed record PeriodoNominaRango(
    Guid Id,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    EstadoPeriodoNomina Estado);

public sealed record ConceptoNominaDefinicion(
    Guid Id,
    string Codigo,
    string Nombre,
    NaturalezaConceptoNomina Naturaleza,
    TipoFormulaConceptoNomina TipoFormula,
    decimal? ValorFijo,
    decimal? PorcentajeSalario,
    bool Activo);

public sealed record EntradaLineaManual(
    Guid ConceptoId,
    string Codigo,
    string Nombre,
    NaturalezaConceptoNomina Naturaleza,
    decimal Importe);

public sealed record LineaLiquidacionCalculada(
    int LineaNumero,
    Guid? ConceptoNominaId,
    string ConceptoCodigo,
    string ConceptoNombre,
    NaturalezaConceptoNomina Naturaleza,
    TipoFormulaConceptoNomina TipoFormula,
    decimal BaseCalculo,
    decimal Tarifa,
    decimal Importe);

public sealed record SnapshotLiquidacionNomina(
    Guid ContratoId,
    string TipoContrato,
    decimal SalarioBase,
    DateOnly FechaInicioContrato,
    DateOnly? FechaFinContrato,
    decimal DiasTrabajados,
    decimal HorasTrabajadas,
    string VersionReglasNomina,
    string? PaqueteLegalCodigo,
    bool AplicaReglasLegalesColombia,
    string VersionSoftware);

public sealed record TotalesLiquidacionNomina(decimal TotalDevengos, decimal TotalDeducciones, decimal NetoPagar);

public sealed record ResultadoLiquidacionNomina(
    SnapshotLiquidacionNomina Snapshot,
    IReadOnlyList<LineaLiquidacionCalculada> Lineas,
    TotalesLiquidacionNomina Totales,
    string? AdvertenciaLegal,
    ResultadoLegalColombiaNomina? DetalleLegalColombia = null);
