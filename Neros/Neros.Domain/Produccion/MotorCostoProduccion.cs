using Neros.Domain.Globalizacion;

namespace Neros.Domain.Produccion;

/// <summary>Costos MP, MO e indirectos (plan §54).</summary>
public static class MotorCostoProduccion
{
    public static ResultadoCostoOrdenProduccion Calcular(EntradaCostoProduccion entrada, PoliticaRedondeo politica)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        if (entrada.CostoMateriaPrima < 0 || entrada.CostoManoObra < 0 || entrada.CostoIndirecto < 0)
            throw new ArgumentOutOfRangeException(nameof(entrada));
        var politicaValidada = politica.Validada();
        var mp = MotorRedondeo.Aplicar(entrada.CostoMateriaPrima, politicaValidada);
        var mo = MotorRedondeo.Aplicar(entrada.CostoManoObra, politicaValidada);
        var cif = MotorRedondeo.Aplicar(entrada.CostoIndirecto, politicaValidada);
        var total = MotorRedondeo.Aplicar(mp + mo + cif, politicaValidada);
        if (entrada.CantidadTerminada <= 0)
            return new ResultadoCostoOrdenProduccion(mp, mo, cif, total, 0m);
        var unitario = MotorRedondeo.Aplicar(total / entrada.CantidadTerminada, politicaValidada);
        return new ResultadoCostoOrdenProduccion(mp, mo, cif, total, unitario);
    }
}
