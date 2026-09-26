namespace Neros.Domain.Contabilidad.Reportes;

public enum TipoReporteFinanciero
{
    SituacionFinanciera,
    EstadoResultados,
    FlujoEfectivo,
    CambiosPatrimonio
}

public enum TipoLineaReporte
{
    Cuenta,
    Formula,
    Subtotal
}

public enum TipoFormulaReporte
{
    SUM,
    SUBTRACT,
    PERCENT,
    VARIATION,
    RATIO
}

public sealed record LineaReporteDefinicion(
    string Codigo,
    string Etiqueta,
    TipoLineaReporte TipoLinea,
    TipoFormulaReporte? Formula,
    string? Operando1Codigo,
    string? Operando2Codigo,
    IReadOnlyList<Guid> CuentasMapeadas);

public sealed record SeccionReporteDefinicion(string Codigo, string Etiqueta, IReadOnlyList<LineaReporteDefinicion> Lineas);

public sealed record DefinicionReporteFinanciero(
    string Codigo,
    TipoReporteFinanciero Tipo,
    IReadOnlyList<SeccionReporteDefinicion> Secciones);

public sealed record ValorReporteLinea(string CodigoLinea, decimal? Valor, bool EsNoAplica);

public sealed record ResultadoReporteFinanciero(
    string CodigoReporte,
    IReadOnlyList<ValorReporteLinea> Valores);

public sealed record ControlEcuacionFinanciera(
    decimal ActivoTotal,
    decimal PasivoTotal,
    decimal PatrimonioTotal,
    decimal Diferencia,
    bool Cumple);
