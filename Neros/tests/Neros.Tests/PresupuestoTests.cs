using Neros.Domain.Globalizacion;
using Neros.Domain.Presupuesto;
using Xunit;

namespace Neros.Tests;

public sealed class PresupuestoTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    [Fact]
    public void Variacion_RealMenosPresupuesto_Plan54()
    {
        var resultado = MotorIndicadoresPresupuesto.EvaluarCelda(new CeldaPresupuesto(1_000_000m, 850_000m), PoliticaCop());
        Assert.Equal(-150_000m, resultado.Variacion);
        Assert.Equal(85m, resultado.EjecucionPorcentaje);
        Assert.False(resultado.EjecucionNoAplica);
    }

    [Fact]
    public void Ejecucion_NaSiPresupuestoEsCero()
    {
        var resultado = MotorIndicadoresPresupuesto.EvaluarCelda(new CeldaPresupuesto(0m, 50_000m), PoliticaCop());
        Assert.True(resultado.EjecucionNoAplica);
        Assert.Null(resultado.EjecucionPorcentaje);
        Assert.Equal(50_000m, resultado.Variacion);
    }

    [Fact]
    public void Consolidado_SumaCeldas()
    {
        var celdas = new[]
        {
            new CeldaPresupuesto(500_000m, 400_000m),
            new CeldaPresupuesto(300_000m, 350_000m)
        };
        var consolidado = MotorIndicadoresPresupuesto.Consolidar(celdas, PoliticaCop());
        Assert.Equal(800_000m, consolidado.TotalPresupuestado);
        Assert.Equal(750_000m, consolidado.TotalReal);
        Assert.Equal(-50_000m, consolidado.Indicadores.Variacion);
    }

    [Fact]
    public void Version_SoloBorradorEsEditable()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MaquinaEstadosVersionPresupuesto.ValidarEditable(EstadoVersionPresupuesto.Aprobada));
        MaquinaEstadosVersionPresupuesto.ValidarEditable(EstadoVersionPresupuesto.Borrador);
        Assert.Equal(EstadoVersionPresupuesto.Aprobada,
            MaquinaEstadosVersionPresupuesto.Aprobar(EstadoVersionPresupuesto.Borrador));
    }
}
