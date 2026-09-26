using Neros.Domain.Globalizacion;
using Neros.Domain.Presupuesto;

namespace Neros.Domain.Proyectos;

/// <summary>Variacion y ejecucion del presupuesto de proyecto (alineado a §54).</summary>
public static class MotorSeguimientoProyecto
{
    public static ResultadoSeguimientoProyecto Evaluar(
        decimal presupuestoTotal,
        decimal totalImputadoGastos,
        PoliticaRedondeo politica)
    {
        var indicadores = MotorIndicadoresPresupuesto.EvaluarCelda(
            new CeldaPresupuesto(presupuestoTotal, totalImputadoGastos), politica);
        return new ResultadoSeguimientoProyecto(
            presupuestoTotal,
            totalImputadoGastos,
            indicadores.Variacion,
            indicadores.EjecucionPorcentaje,
            indicadores.EjecucionNoAplica);
    }
}
