using Neros.Domain.Globalizacion;
using Neros.Domain.Nomina.Legal;

namespace Neros.Domain.Nomina;

/// <summary>Aportes de seguridad social (Colombia parametrizada).</summary>
public static class MotorSeguridadSocialNomina
{
    public static decimal BaseCotizacion(decimal salarioBase, decimal totalDevengosFijos) =>
        Math.Max(salarioBase, totalDevengosFijos);

    public static ResultadoSeguridadSocialNomina CalcularAportes(
        decimal baseCotizacion,
        ParametrosLegalesColombia parametros,
        PoliticaRedondeo politica)
    {
        if (baseCotizacion < 0) throw new ArgumentOutOfRangeException(nameof(baseCotizacion));
        ArgumentNullException.ThrowIfNull(parametros);
        var baseRedondeada = MotorRedondeo.Aplicar(baseCotizacion, politica);
        decimal Pct(decimal pct) => MotorRedondeo.Aplicar(baseRedondeada * pct / 100m, politica);
        return new ResultadoSeguridadSocialNomina(
            baseRedondeada,
            Pct(parametros.TarifaSaludEmpleadoPct),
            Pct(parametros.TarifaPensionEmpleadoPct),
            Pct(parametros.TarifaSaludEmpleadorPct),
            Pct(parametros.TarifaPensionEmpleadorPct),
            Pct(parametros.TarifaArlEmpleadorPct),
            Pct(parametros.TarifaCajaEmpleadorPct));
    }
}
