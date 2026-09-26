using Neros.Domain.Globalizacion;

namespace Neros.Domain.Contabilidad.Reportes;

public static class MotorReportesFinancieros
{
    public static ResultadoReporteFinanciero Generar(
        DefinicionReporteFinanciero definicion,
        IReadOnlyDictionary<Guid, decimal> saldosPorCuenta,
        PoliticaRedondeo politica)
    {
        ArgumentNullException.ThrowIfNull(definicion);
        var acumulado = new Dictionary<string, decimal?>(StringComparer.Ordinal);
        var salida = new List<ValorReporteLinea>();
        foreach (var seccion in definicion.Secciones.OrderBy(s => s.Codigo))
        {
            foreach (var linea in seccion.Lineas)
            {
                var valor = linea.TipoLinea switch
                {
                    TipoLineaReporte.Cuenta => EvaluarCuenta(linea, saldosPorCuenta, politica),
                    TipoLineaReporte.Formula => MotorFormulasReporte.Evaluar(linea, acumulado, politica),
                    TipoLineaReporte.Subtotal => MotorFormulasReporte.Evaluar(
                        linea with { TipoLinea = TipoLineaReporte.Formula, Formula = TipoFormulaReporte.SUM },
                        acumulado, politica),
                    _ => throw new ArgumentOutOfRangeException(nameof(linea))
                };
                acumulado[linea.Codigo] = valor.Valor;
                salida.Add(valor);
            }
        }
        return new ResultadoReporteFinanciero(definicion.Codigo, salida);
    }

    public static ControlEcuacionFinanciera ControlSituacionFinanciera(
        IReadOnlyDictionary<Guid, CuentaContable> cuentas,
        IReadOnlyDictionary<Guid, decimal> saldosFirmados,
        bool despuesCierreResultados)
    {
        var ecuacion = EcuacionContable.Evaluar(cuentas, saldosFirmados, despuesCierreResultados);
        var pasivoPatrimonio = ecuacion.Pasivo + ecuacion.Patrimonio;
        if (!despuesCierreResultados)
            pasivoPatrimonio += ecuacion.Ingresos - ecuacion.Costos - ecuacion.Gastos;
        var diferencia = ecuacion.Activo - pasivoPatrimonio;
        return new ControlEcuacionFinanciera(ecuacion.Activo, ecuacion.Pasivo, ecuacion.Patrimonio, diferencia, ecuacion.Cumple);
    }

    private static ValorReporteLinea EvaluarCuenta(
        LineaReporteDefinicion linea, IReadOnlyDictionary<Guid, decimal> saldos, PoliticaRedondeo politica)
    {
        var total = MotorFormulasReporte.SumarCuentas(linea.CuentasMapeadas, saldos);
        var redondeado = MotorRedondeo.Aplicar(total, politica.Validada());
        return new ValorReporteLinea(linea.Codigo, redondeado, false);
    }
}
