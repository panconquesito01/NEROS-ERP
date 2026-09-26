using Neros.Domain.Globalizacion;

namespace Neros.Domain.Presupuesto;

/// <summary>Variacion y ejecucion % (plan §54).</summary>
public static class MotorIndicadoresPresupuesto
{
    public static ResultadoIndicadoresPresupuesto EvaluarCelda(
        CeldaPresupuesto celda,
        PoliticaRedondeo politica)
    {
        var politicaValidada = politica.Validada();
        var variacion = MotorRedondeo.Aplicar(celda.ValorReal - celda.ValorPresupuestado, politicaValidada);
        if (celda.ValorPresupuestado == 0)
            return new ResultadoIndicadoresPresupuesto(variacion, null, true);
        var ejecucion = MotorRedondeo.Aplicar(celda.ValorReal / celda.ValorPresupuestado * 100m, politicaValidada);
        return new ResultadoIndicadoresPresupuesto(variacion, ejecucion, false);
    }

    public static ResultadoConsolidadoPresupuesto Consolidar(
        IEnumerable<CeldaPresupuesto> celdas,
        PoliticaRedondeo politica)
    {
        var lista = celdas.ToList();
        var presupuesto = lista.Sum(c => c.ValorPresupuestado);
        var real = lista.Sum(c => c.ValorReal);
        var indicadores = EvaluarCelda(new CeldaPresupuesto(presupuesto, real), politica);
        return new ResultadoConsolidadoPresupuesto(presupuesto, real, indicadores);
    }
}
