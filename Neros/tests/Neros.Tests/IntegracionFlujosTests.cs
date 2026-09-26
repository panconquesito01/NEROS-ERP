using Neros.Domain.Contabilidad;
using Neros.Domain.Globalizacion;
using Neros.Domain.Integracion;
using Neros.Domain.Inventario;
using Xunit;

namespace Neros.Tests;

public sealed class IntegracionFlujosTests
{
    [Fact]
    public void PedidoConfirmado_GeneraReservaPorLinea()
    {
        var producto = Guid.NewGuid();
        var bodega = Guid.NewGuid();
        var pedido = new PedidoConfirmadoIntegracion(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1,
            [new LineaPedidoParaReserva(producto, bodega, 3)]);
        var reserva = FlujosIntegracion.PedidoConfirmadoAReserva(pedido);
        Assert.Equal(pedido.PedidoId, reserva.PedidoId);
        Assert.Single(reserva.Lineas);
        Assert.Equal(3, reserva.Lineas[0].Cantidad);
    }

    [Fact]
    public void Recepcion_GeneraEntradasDeInventario()
    {
        var entrada = FlujosIntegracion.MapearRecepcion(new RecepcionCompraIntegracion(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1,
            [new LineaRecepcionParaInventario(Guid.NewGuid(), Guid.NewGuid(), 10, 5000m)]));
        var movimientos = FlujosIntegracion.RecepcionAEntradasInventario(entrada, new DateTime(2026, 9, 26, 0, 0, 0, DateTimeKind.Utc));
        Assert.Single(movimientos);
        Assert.Equal(TipoMovimientoInventario.Entrada, movimientos[0].Tipo);
        Assert.True(movimientos[0].EsEntrada);
    }

    [Fact]
    public void Recepcion_GeneraReglasContablesCuadradas()
    {
        var reglas = FlujosIntegracion.RecepcionAReglasContables(
            new ContabilizacionPorRecepcion(Guid.NewGuid(), Guid.NewGuid(), 1_190_000m, 1_000_000m, 190_000m),
            CatalogoIso.PoliticaPorDefecto("COP"));
        var movimientos = MotorContabilizacion.ResolverMovimientos(reglas, new Dictionary<RolContable, Guid>
        {
            [RolContable.CuentaInventario] = Guid.NewGuid(),
            [RolContable.CuentaIvaDescontable] = Guid.NewGuid(),
            [RolContable.CuentaProveedor] = Guid.NewGuid()
        });
        Assert.Equal(
            movimientos.Where(m => m.Lado == LadoMovimiento.Debito).Sum(m => m.ImporteMonedaFuncional),
            movimientos.Where(m => m.Lado == LadoMovimiento.Credito).Sum(m => m.ImporteMonedaFuncional));
    }

    [Fact]
    public void Nomina_GeneraReglasContablesCuadradas()
    {
        var reglas = FlujosIntegracion.NominaAReglasContables(
            new ContabilizacionPorNomina(Guid.NewGuid(), Guid.NewGuid(), 5_000_000m, 500_000m, 4_500_000m),
            CatalogoIso.PoliticaPorDefecto("COP"));
        var movimientos = MotorContabilizacion.ResolverMovimientos(reglas, new Dictionary<RolContable, Guid>
        {
            [RolContable.CuentaGastoNomina] = Guid.NewGuid(),
            [RolContable.CuentaRetencionNomina] = Guid.NewGuid(),
            [RolContable.CuentaObligacionNomina] = Guid.NewGuid()
        });
        Assert.Equal(
            movimientos.Where(m => m.Lado == LadoMovimiento.Debito).Sum(m => m.ImporteMonedaFuncional),
            movimientos.Where(m => m.Lado == LadoMovimiento.Credito).Sum(m => m.ImporteMonedaFuncional));
    }

    [Fact]
    public void Ordenador_RechazaVersionAntigua_Desorden()
    {
        var ordenador = new OrdenadorVersionAgregado();
        Assert.True(ordenador.DebeProcesar("pedido-1", 2));
        Assert.False(ordenador.DebeProcesar("pedido-1", 1));
        Assert.True(ordenador.DebeProcesar("pedido-1", 3));
    }
}
