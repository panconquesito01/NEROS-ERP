using Neros.Domain.Globalizacion;
using Neros.Domain.Nomina;

namespace Neros.Domain.Nomina.Legal;

/// <summary>Motor legal Colombia (parametrizado; requiere validacion de especialista en produccion).</summary>
public static class MotorLegalColombiaNomina
{
    public static ResultadoLegalColombiaNomina Calcular(
        decimal totalDevengos,
        decimal salarioBase,
        ParametrosLegalesColombia parametros,
        DateOnly fechaLiquidacion,
        PoliticaRedondeo politica)
    {
        MotorPaqueteLegalColombia.ValidarParaCalculo(parametros, fechaLiquidacion);
        var politicaValidada = politica.Validada();
        var baseCotizacion = MotorSeguridadSocialNomina.BaseCotizacion(salarioBase, totalDevengos);
        var ss = MotorSeguridadSocialNomina.CalcularAportes(baseCotizacion, parametros, politicaValidada);
        var retencion = MotorTributarioNomina.CalcularRetencionFuente(totalDevengos, parametros, politicaValidada);

        var deducciones = new List<LineaDeduccionLegal>
        {
            CrearLinea("SS-SALUD-EMP", "Aporte salud empleado", baseCotizacion, parametros.TarifaSaludEmpleadoPct, ss.AporteSaludEmpleado),
            CrearLinea("SS-PENSION-EMP", "Aporte pension empleado", baseCotizacion, parametros.TarifaPensionEmpleadoPct, ss.AportePensionEmpleado)
        };
        if (retencion.ImporteRetencion > 0)
            deducciones.Add(CrearLinea("RET-FTE", "Retencion en la fuente", retencion.BaseRetencion, parametros.TarifaRetencionFuentePct, retencion.ImporteRetencion));

        return new ResultadoLegalColombiaNomina(ss, retencion, deducciones);
    }

    public static IReadOnlyList<LineaLiquidacionCalculada> ConvertirDeduccionesALineas(
        IReadOnlyList<LineaDeduccionLegal> deducciones,
        int lineaInicio)
    {
        var salida = new List<LineaLiquidacionCalculada>();
        var numero = lineaInicio;
        foreach (var d in deducciones)
        {
            salida.Add(new LineaLiquidacionCalculada(
                numero++, null, d.CodigoConcepto, d.NombreConcepto,
                NaturalezaConceptoNomina.Deduccion, TipoFormulaConceptoNomina.Manual,
                d.BaseCalculo, d.Tarifa, d.Importe));
        }
        return salida;
    }

    private static LineaDeduccionLegal CrearLinea(string codigo, string nombre, decimal baseCalculo, decimal tarifa, decimal importe) =>
        new(codigo, nombre, baseCalculo, tarifa, importe);
}
