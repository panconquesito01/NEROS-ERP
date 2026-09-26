using Neros.Domain.Globalizacion;
using Neros.Domain.Produccion;
using Xunit;

namespace Neros.Tests;

public sealed class ProduccionTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static readonly Guid CompA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CompB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void ListaMateriales_ExplosionaCantidades()
    {
        var lista = new ListaMaterialesDefinicion(Guid.NewGuid(), 10m,
        [
            new LineaListaMateriales(CompA, 2m),
            new LineaListaMateriales(CompB, 1m)
        ]);
        var req = MotorListaMateriales.Explosionar(lista, 50m);
        Assert.Equal(10m, req.Single(r => r.ComponenteReferenciaId == CompA).CantidadRequerida);
        Assert.Equal(5m, req.Single(r => r.ComponenteReferenciaId == CompB).CantidadRequerida);
    }

    [Fact]
    public void Movimientos_ConsumoDevolucionYTopePlanificado()
    {
        var saldos = new SaldosOrdenProduccion(100m, 0m, 0m, 0m, 0m);
        saldos = MotorMovimientosProduccion.AplicarMovimiento(
            saldos, EstadoOrdenProduccion.EnProceso, TipoMovimientoProduccion.Consumo, 40m);
        saldos = MotorMovimientosProduccion.AplicarMovimiento(
            saldos, EstadoOrdenProduccion.EnProceso, TipoMovimientoProduccion.Devolucion, 5m);
        Assert.Equal(35m, MotorMovimientosProduccion.ConsumoNetoComponente(saldos));
        saldos = MotorMovimientosProduccion.AplicarMovimiento(
            saldos, EstadoOrdenProduccion.EnProceso, TipoMovimientoProduccion.Terminado, 90m);
        Assert.Throws<InvalidOperationException>(() => MotorMovimientosProduccion.AplicarMovimiento(
            saldos, EstadoOrdenProduccion.EnProceso, TipoMovimientoProduccion.Merma, 20m));
    }

    [Fact]
    public void CostoProduccion_DesglosaMpMoCif()
    {
        var resultado = MotorCostoProduccion.Calcular(
            new EntradaCostoProduccion(500_000m, 200_000m, 50_000m, 10m), PoliticaCop());
        Assert.Equal(750_000m, resultado.CostoTotal);
        Assert.Equal(75_000m, resultado.CostoUnitarioTerminado);
    }

    [Fact]
    public void Orden_FlujoEstadosBasico()
    {
        var estado = MaquinaEstadosOrdenProduccion.Liberar(EstadoOrdenProduccion.Planificada);
        estado = MaquinaEstadosOrdenProduccion.Iniciar(estado);
        Assert.Equal(EstadoOrdenProduccion.EnProceso, estado);
        estado = MaquinaEstadosOrdenProduccion.Terminar(estado);
        Assert.Equal(EstadoOrdenProduccion.Cerrada, MaquinaEstadosOrdenProduccion.Cerrar(estado));
    }
}
