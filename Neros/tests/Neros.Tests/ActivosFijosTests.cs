using Neros.Domain.Activos;
using Neros.Domain.Globalizacion;
using Xunit;

namespace Neros.Tests;

public sealed class ActivosFijosTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static ActivoFijoParametros ActivoEnServicio() => new(
        Guid.NewGuid(), 12_000_000m, 2_000_000m, 60, MetodoDepreciacionActivo.Lineal,
        EstadoActivoFijo.Activo, 0m, 0m);

    [Fact]
    public void DepreciacionLineal_CuotaMensual_Plan54()
    {
        var cuota = MotorDepreciacionLineal.CuotaMensualBruta(12_000_000m, 2_000_000m, 60);
        Assert.Equal(166_666.666666666666666666666667m, cuota);
        var resultado = MotorDepreciacionLineal.CalcularCuotaPeriodo(ActivoEnServicio(), mesesDepreciadosAntes: 0, PoliticaCop());
        Assert.Equal(166_666.67m, resultado.ImportePeriodo);
        Assert.Equal(11_833_333.33m, resultado.ValorEnLibros);
    }

    [Fact]
    public void Depreciacion_UltimoPeriodo_NoSuperaBaseDepreciable()
    {
        var activo = ActivoEnServicio() with { DepreciacionAcumulada = 9_800_000m };
        var resultado = MotorDepreciacionLineal.CalcularCuotaPeriodo(activo, mesesDepreciadosAntes: 59, PoliticaCop());
        Assert.Equal(200_000m, resultado.ImportePeriodo);
        Assert.True(resultado.UltimoPeriodo);
        Assert.Equal(2_000_000m, resultado.ValorEnLibros);
    }

    [Fact]
    public void Deterioro_NoSuperaValorEnLibros()
    {
        var activo = ActivoEnServicio() with { DepreciacionAcumulada = 4_000_000m };
        var deterioro = MotorDeterioroActivo.Registrar(activo, 500_000m, PoliticaCop());
        Assert.Equal(7_500_000m, deterioro.ValorEnLibros);
        Assert.Throws<InvalidOperationException>(() => MotorDeterioroActivo.Registrar(activo, 9_000_000m, PoliticaCop()));
    }

    [Fact]
    public void MaquinaEstados_ActivoYBaja()
    {
        var activo = MaquinaEstadosActivoFijo.Activar(EstadoActivoFijo.Borrador);
        Assert.Equal(EstadoActivoFijo.Activo, activo);
        Assert.Equal(EstadoActivoFijo.DadoDeBaja, MaquinaEstadosActivoFijo.DarDeBaja(activo));
    }
}
