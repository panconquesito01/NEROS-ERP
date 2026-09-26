using Neros.Domain.Globalizacion;

namespace Neros.Domain.Impuestos;

/// <summary>Motor puro de impuestos (plan §38–§40). Sin acceso a base de datos ni red.</summary>
public static class MotorImpuestos
{
    public static (decimal BaseGravable, decimal ImporteImpuesto) CalcularPorcentajeSobreBase(
        decimal baseGravable, decimal tarifa, PoliticaRedondeo politica)
    {
        if (baseGravable < 0) throw new ArgumentOutOfRangeException(nameof(baseGravable));
        if (tarifa < 0) throw new ArgumentOutOfRangeException(nameof(tarifa));
        var baseRedondeada = MotorRedondeo.Aplicar(baseGravable, politica.Validada());
        var impuesto = MotorRedondeo.Aplicar(baseRedondeada * tarifa, politica.Validada());
        return (baseRedondeada, impuesto);
    }

    /// <summary>Impuesto incluido con un solo impuesto porcentual (plan §39).</summary>
    public static (decimal BaseGravable, decimal ImporteImpuesto) CalcularImpuestoIncluido(
        decimal totalConImpuesto, decimal tarifa, PoliticaRedondeo politica)
    {
        if (totalConImpuesto < 0) throw new ArgumentOutOfRangeException(nameof(totalConImpuesto));
        if (tarifa < 0) throw new ArgumentOutOfRangeException(nameof(tarifa));
        var politicaValidada = politica.Validada();
        var divisor = 1 + tarifa;
        if (divisor <= 0) throw new ArgumentOutOfRangeException(nameof(tarifa));
        var baseGravable = MotorRedondeo.Aplicar(totalConImpuesto / divisor, politicaValidada);
        var impuesto = totalConImpuesto - baseGravable;
        return (baseGravable, impuesto);
    }

    public static decimal CalcularRetencion(decimal baseSujeta, decimal tarifaRetencion, PoliticaRedondeo politica)
    {
        if (baseSujeta < 0) throw new ArgumentOutOfRangeException(nameof(baseSujeta));
        var (_, importe) = CalcularPorcentajeSobreBase(baseSujeta, tarifaRetencion, politica);
        return importe;
    }

    public static LineaCalculada CalcularLinea(
        LineaEntrada linea, VersionImpuestosInmutable version, PoliticaRedondeo politica, bool precioUnitarioIncluyeImpuesto = false)
    {
        ArgumentNullException.ThrowIfNull(linea);
        var reglas = version.Validada().ReglasOrdenadas.Where(r => !r.EsRetencion).ToList();
        var bruto = MotorRedondeo.Aplicar(linea.Cantidad * linea.PrecioUnitario, politica.Validada());
        var descuentos = MotorRedondeo.Aplicar(Math.Max(0, linea.DescuentoLinea), politica.Validada());
        var baseNeta = MotorRedondeo.Aplicar(Math.Max(0, bruto - descuentos), politica.Validada());
        if (reglas.Count == 0)
            return new LineaCalculada(bruto, descuentos, baseNeta, [], CalcularRetencionesLinea(baseNeta, version, politica));

        if (precioUnitarioIncluyeImpuesto && reglas.Count == 1 && reglas[0].PrecioIncluyeImpuesto)
        {
            var regla = reglas[0];
            var (baseIncluida, impuesto) = CalcularImpuestoIncluido(baseNeta, regla.Tarifa, politica);
            var impuestos = new[] { new ResultadoImpuesto(regla.CodigoImpuesto, baseIncluida, regla.Tarifa, impuesto, false) };
            return new LineaCalculada(bruto, descuentos, baseIncluida, impuestos, CalcularRetencionesLinea(baseIncluida, version, politica));
        }

        var acumuladoImpuestos = 0m;
        var resultados = new List<ResultadoImpuesto>();
        foreach (var regla in reglas)
        {
            var baseRegla = regla.BaseAcumulaImpuestosPrevios ? baseNeta + acumuladoImpuestos : baseNeta;
            var (baseGravable, importe) = CalcularPorcentajeSobreBase(baseRegla, regla.Tarifa, politica);
            resultados.Add(new ResultadoImpuesto(regla.CodigoImpuesto, baseGravable, regla.Tarifa, importe, false));
            acumuladoImpuestos += importe;
        }
        return new LineaCalculada(bruto, descuentos, baseNeta, resultados, CalcularRetencionesLinea(baseNeta, version, politica));
    }

    public static DocumentoCalculado CalcularDocumento(
        IReadOnlyList<LineaEntrada> lineas, VersionImpuestosInmutable version, PoliticaRedondeo politica,
        decimal anticiposAplicados = 0, bool precioUnitarioIncluyeImpuesto = false)
    {
        if (lineas.Count == 0) return DocumentoCalculado.Vacio();
        var calculadas = lineas.Select(l => CalcularLinea(l, version, politica, precioUnitarioIncluyeImpuesto)).ToList();
        var subtotal = MotorRedondeo.Aplicar(calculadas.Sum(l => l.BaseNeta), politica.Validada());
        var totalImpuestos = MotorRedondeo.Aplicar(calculadas.Sum(l => l.TotalImpuestos), politica.Validada());
        var totalRetenciones = MotorRedondeo.Aplicar(calculadas.Sum(l => l.TotalRetenciones), politica.Validada());
        var total = MotorRedondeo.Aplicar(subtotal + totalImpuestos, politica.Validada());
        var anticipos = MotorRedondeo.Aplicar(Math.Max(0, anticiposAplicados), politica.Validada());
        var valorAPagar = MotorRedondeo.Aplicar(total - totalRetenciones - anticipos, politica.Validada());
        return new DocumentoCalculado(calculadas, subtotal, totalImpuestos, totalRetenciones, total, valorAPagar);
    }

    private static IReadOnlyList<ResultadoImpuesto> CalcularRetencionesLinea(
        decimal baseNeta, VersionImpuestosInmutable version, PoliticaRedondeo politica)
    {
        var retenciones = new List<ResultadoImpuesto>();
        foreach (var regla in version.Validada().ReglasOrdenadas.Where(r => r.EsRetencion))
        {
            var importe = CalcularRetencion(baseNeta, regla.Tarifa, politica);
            if (importe == 0) continue;
            retenciones.Add(new ResultadoImpuesto(regla.CodigoImpuesto, baseNeta, regla.Tarifa, importe, true));
        }
        return retenciones;
    }
}
