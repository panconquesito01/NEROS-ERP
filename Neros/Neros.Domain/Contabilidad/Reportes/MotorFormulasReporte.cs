using Neros.Domain.Globalizacion;

namespace Neros.Domain.Contabilidad.Reportes;

/// <summary>Lenguaje cerrado de formulas (plan §41).</summary>
public static class MotorFormulasReporte
{
    public static ValorReporteLinea Evaluar(
        LineaReporteDefinicion linea,
        IReadOnlyDictionary<string, decimal?> valoresPorCodigo,
        PoliticaRedondeo politica)
    {
        ArgumentNullException.ThrowIfNull(linea);
        if (linea.TipoLinea == TipoLineaReporte.Cuenta)
            throw new InvalidOperationException("Use EvaluarLineaCuenta para lineas de cuenta.");
        if (linea.Formula is null)
            throw new InvalidOperationException($"La linea {linea.Codigo} requiere formula.");
        var valor = linea.Formula switch
        {
            TipoFormulaReporte.SUM => SumarOperandos(linea, valoresPorCodigo),
            TipoFormulaReporte.SUBTRACT => Restar(linea, valoresPorCodigo),
            TipoFormulaReporte.PERCENT => Porcentaje(linea, valoresPorCodigo),
            TipoFormulaReporte.VARIATION => Variacion(linea, valoresPorCodigo),
            TipoFormulaReporte.RATIO => Ratio(linea, valoresPorCodigo),
            _ => throw new ArgumentOutOfRangeException(nameof(linea))
        };
        if (!valor.HasValue)
            return new ValorReporteLinea(linea.Codigo, null, true);
        var redondeado = MotorRedondeo.Aplicar(valor.Value, politica.Validada());
        return new ValorReporteLinea(linea.Codigo, redondeado, false);
    }

    public static decimal SumarCuentas(IReadOnlyList<Guid> cuentas, IReadOnlyDictionary<Guid, decimal> saldos)
        => cuentas.Sum(id => saldos.GetValueOrDefault(id));

    private static decimal? SumarOperandos(LineaReporteDefinicion linea, IReadOnlyDictionary<string, decimal?> valores)
    {
        var codigos = Operandos(linea);
        if (codigos.Count == 0) return 0;
        decimal total = 0;
        foreach (var codigo in codigos)
        {
            if (!valores.TryGetValue(codigo, out var v) || v is null) return null;
            total += v.Value;
        }
        return total;
    }

    private static decimal? Restar(LineaReporteDefinicion linea, IReadOnlyDictionary<string, decimal?> valores)
    {
        var (a, b) = DosOperandos(linea);
        if (!valores.TryGetValue(a, out var va) || va is null) return null;
        if (!valores.TryGetValue(b, out var vb) || vb is null) return null;
        return va - vb;
    }

    private static decimal? Porcentaje(LineaReporteDefinicion linea, IReadOnlyDictionary<string, decimal?> valores)
    {
        var (num, den) = DosOperandos(linea);
        if (!valores.TryGetValue(num, out var vn) || vn is null) return null;
        if (!valores.TryGetValue(den, out var vd) || vd is null || vd == 0) return null;
        return vn / vd * 100m;
    }

    private static decimal? Variacion(LineaReporteDefinicion linea, IReadOnlyDictionary<string, decimal?> valores)
        => Restar(linea, valores);

    private static decimal? Ratio(LineaReporteDefinicion linea, IReadOnlyDictionary<string, decimal?> valores)
    {
        var (num, den) = DosOperandos(linea);
        if (!valores.TryGetValue(num, out var vn) || vn is null) return null;
        if (!valores.TryGetValue(den, out var vd) || vd is null || vd == 0) return null;
        return vn / vd;
    }

    private static (string A, string B) DosOperandos(LineaReporteDefinicion linea)
    {
        if (string.IsNullOrWhiteSpace(linea.Operando1Codigo) || string.IsNullOrWhiteSpace(linea.Operando2Codigo))
            throw new InvalidOperationException($"La formula {linea.Formula} requiere dos operandos.");
        return (linea.Operando1Codigo, linea.Operando2Codigo);
    }

    private static IReadOnlyList<string> Operandos(LineaReporteDefinicion linea)
    {
        var lista = new List<string>();
        if (!string.IsNullOrWhiteSpace(linea.Operando1Codigo)) lista.Add(linea.Operando1Codigo);
        if (!string.IsNullOrWhiteSpace(linea.Operando2Codigo)) lista.Add(linea.Operando2Codigo);
        return lista;
    }
}
