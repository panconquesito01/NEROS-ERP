using Neros.Domain.Globalizacion;
using Xunit;

namespace Neros.Tests;

public sealed class GlobalizacionDominioTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    [Theory]
    [InlineData(1.005, 1.01)]
    [InlineData(-1.005, -1.01)]
    public void RedondeoAwayFromZeroDosDecimales(decimal entrada, decimal esperado)
    {
        var resultado = MotorRedondeo.Redondear(entrada, 2, ModoRedondeo.AwayFromZero);
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData(2.5, 2)]
    [InlineData(3.5, 4)]
    public void RedondeoToEven(decimal entrada, decimal esperado)
    {
        Assert.Equal(esperado, MotorRedondeo.Redondear(entrada, 0, ModoRedondeo.ToEven));
    }

    [Fact]
    public void TruncarNoRedondeaHaciaArriba()
    {
        Assert.Equal(1.23m, MotorRedondeo.Redondear(1.239m, 2, ModoRedondeo.Truncate));
    }

    [Fact]
    public void DineroAplicaPoliticaPorLinea()
    {
        var importe = new Dinero(10.125m, "COP");
        var redondeado = importe.Redondear(PoliticaCop());
        Assert.Equal(10.13m, redondeado.Cantidad);
    }

    [Fact]
    public void ConversionUsdACopUsaTasaYPoliticaDestino()
    {
        var origen = new Dinero(100m, "USD");
        var politicaCop = PoliticaCop();
        var convertido = ConversionMoneda.Convertir(origen, "COP", 4200m, politicaCop);
        Assert.Equal("COP", convertido.Moneda);
        Assert.Equal(420_000m, convertido.Cantidad);
    }

    [Fact]
    public void ConversionConDecimalesIntermedios()
    {
        var origen = new Dinero(10.33m, "USD");
        var convertido = ConversionMoneda.Convertir(origen, "COP", 4200.333333m, PoliticaCop());
        Assert.Equal(43_389.44m, convertido.Cantidad);
    }

    [Fact]
    public void FechaNegocioRespetaZonaEmpresa()
    {
        var instante = new DateTime(2026, 3, 15, 4, 30, 0, DateTimeKind.Utc);
        Assert.Equal(new DateOnly(2026, 3, 14), FechasNegocio.DesdeInstanteUtc(instante, "America/Bogota"));
        Assert.Equal(new DateOnly(2026, 3, 15), FechasNegocio.DesdeInstanteUtc(instante, "Europe/Madrid"));
    }

    [Fact]
    public void MonedaDesconocidaRechazada()
    {
        Assert.Throws<ArgumentException>(() => CatalogoIso.ValidarMoneda("XXX"));
    }
}
