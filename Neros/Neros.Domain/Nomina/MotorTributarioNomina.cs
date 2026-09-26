using Neros.Domain.Globalizacion;
using Neros.Domain.Nomina.Legal;

namespace Neros.Domain.Nomina;

/// <summary>Retencion en la fuente parametrizada (Colombia).</summary>
public static class MotorTributarioNomina
{
    public static decimal BaseRetencionFuente(decimal totalDevengos) =>
        totalDevengos < 0 ? throw new ArgumentOutOfRangeException(nameof(totalDevengos)) : totalDevengos;

    public static ResultadoRetencionFuenteNomina CalcularRetencionFuente(
        decimal totalDevengos,
        ParametrosLegalesColombia parametros,
        PoliticaRedondeo politica)
    {
        var baseRetencion = BaseRetencionFuente(totalDevengos);
        if (baseRetencion <= parametros.UmbralRetencionFuente || parametros.TarifaRetencionFuentePct <= 0)
            return new ResultadoRetencionFuenteNomina(baseRetencion, 0m);
        var importe = MotorRedondeo.Aplicar(baseRetencion * parametros.TarifaRetencionFuentePct / 100m, politica);
        return new ResultadoRetencionFuenteNomina(baseRetencion, importe);
    }
}
